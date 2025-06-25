using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace ZenGarden.Core.Services;

public class UserService(
    IUserRepository userRepository,
    IBagRepository bagRepository,
    IWalletRepository walletRepository,
    IUserXpConfigRepository userXpConfigRepository,
    IUserExperienceRepository userExperienceRepository,
    IUserConfigRepository userConfigRepository,
    IBagItemRepository bagItemRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRedisService redisService,
    IServiceScopeFactory scopeFactory) : IUserService
{
    private const int CacheDurationMinutes = 30;

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return mapper.Map<List<UserDto>>(users);
    }

    public async Task<Users?> GetUserByIdAsync(int userId)
    {
        string cacheKey = $"user:id:{userId}";
        
        var cachedUser = await redisService.GetAsync<UserResponseDto>(cacheKey);
        if (cachedUser != null)
        {
            return await userRepository.GetByIdAsync(cachedUser.UserId);
        }

        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        var userDto = mapper.Map<UserResponseDto>(user);
        await redisService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(CacheDurationMinutes));

        return user;
    }

    public async Task<Users?> GetUserByEmailAsync(string email)
    {
        string cacheKey = $"user:email:{email}";
        
        var user = await redisService.GetOrSetAsync(
            cacheKey,
            async () => await userRepository.GetByEmailAsync(email),
            TimeSpan.FromMinutes(CacheDurationMinutes)
        );

        return user;
    }

    public async Task UpdateUserAsync(UpdateUserDTO user)
    {
        var userUpdate = await GetUserByIdAsync(user.UserId);
        if (userUpdate == null)
            throw new KeyNotFoundException($"User with ID {user.UserId} not found.");
        
        if (!string.IsNullOrEmpty(user.Email))
            userUpdate.Email = user.Email;
        if (!string.IsNullOrEmpty(user.Phone))
            userUpdate.Phone = user.Phone;
        if (!string.IsNullOrEmpty(user.UserName))
            userUpdate.UserName = user.UserName;

        userRepository.Update(userUpdate);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update user.");

        // Invalidate cache
        await redisService.RemoveAsync($"user:id:{user.UserId}");
        await redisService.RemoveAsync($"user:email:{userUpdate.Email}");
        if (!string.IsNullOrEmpty(userUpdate.Phone))
            await redisService.RemoveAsync($"user:phone:{userUpdate.Phone}");
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        await userRepository.RemoveAsync(user);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete user.");
    }

    public async Task<Users?> ValidateUserAsync(string? email, string? phone, string password)
    {
        string cacheKey = !string.IsNullOrEmpty(email) 
            ? $"user:email:{email}" 
            : $"user:phone:{phone}";

        // Try to get user directly from database first
        var user = !string.IsNullOrEmpty(email)
            ? await userRepository.GetByEmailAsync(email)
            : await userRepository.GetByPhoneAsync(phone);

        if (user == null || string.IsNullOrEmpty(user.Password)) return null;

        if (PasswordHasher.VerifyPassword(password, user.Password))
        {
            // Cache the user after successful validation
            var userDto = mapper.Map<UserResponseDto>(user);
            await redisService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(CacheDurationMinutes));
            return user;
        }

        return null;
    }

    public async Task<Users?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await userRepository.GetUserByRefreshTokenAsync(refreshToken);
    }

    public async Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
    {
        await userRepository.UpdateUserRefreshTokenAsync(userId, refreshToken, expiryDate);
    }

    public async Task RemoveRefreshTokenAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException("User not found.");

        user.RefreshTokenHash = null;
        user.RefreshTokenExpiry = DateTime.UtcNow;

        await unitOfWork.CommitAsync();
    }

    public async Task<Users?> RegisterUserAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password cannot be empty.");

        // Check if email or phone already exists
        var existingUserByEmail = await userRepository.GetByEmailAsync(dto.Email);
        if (existingUserByEmail != null) 
            throw new InvalidOperationException("Email is already in use.");

        var existingUserByPhone = await userRepository.GetByPhoneAsync(dto.Phone);
        if (existingUserByPhone != null)
            throw new InvalidOperationException("Phone number is already in use.");

        var role = await userRepository.GetRoleByIdAsync(dto.RoleId ?? 2)
                   ?? throw new InvalidOperationException("Invalid RoleId.");

        var newUser = mapper.Map<Users>(dto);

        newUser.UserName = string.IsNullOrWhiteSpace(dto.UserName) ? GenerateRandomUsername() : dto.UserName;
        if (await userRepository.ExistsByUserNameAsync(newUser.UserName))
            throw new InvalidOperationException("Username is already taken.");

        newUser.Password = PasswordHasher.HashPassword(dto.Password);
        newUser.RoleId = role.RoleId;
        newUser.Status = UserStatus.Active;

        var executionStrategy = unitOfWork.CreateExecutionStrategy();
        
        return await executionStrategy.ExecuteAsync<Users, Users?>(
            newUser,
            async (_, state, _) =>
            {
                // Create user first
                await userRepository.CreateAsync(state);
                await unitOfWork.CommitAsync(); // Commit to get UserId

                // Create related entities
                var wallet = new Wallet { UserId = state.UserId, Balance = 0 };
                var bag = new Bag { UserId = state.UserId };

                var levelOneConfig = await userXpConfigRepository.GetByIdAsync(1)
                                     ?? throw new Exception("UserLevelConfig is missing Level 1!");

                var userExperience = new UserExperience
                {
                    UserId = state.UserId,
                    TotalXp = 0,
                    XpToNextLevel = levelOneConfig.XpThreshold,
                    LevelId = 1,
                    IsMaxLevel = false,
                    StreakDays = 0,
                    CreatedAt = DateTime.UtcNow
                };

                var userConfig = new UserConfig
                {
                    UserId = state.UserId,
                    BackgroundConfig = "https://hcm.ss.bfcplatform.vn/zengarden/OIP.jpg?AWSAccessKeyId=8QEUOTPT6CM3J3X9CD9T&Expires=1743441883&Signature=LNL9h7zMH2%2BeZpQdBdDhnoCVi1k%3D",
                    SoundConfig = "https://hcm.ss.bfcplatform.vn/zengarden/sound-design-elements-sfx-ps-022-302865.mp3?AWSAccessKeyId=8QEUOTPT6CM3J3X9CD9T&Expires=1743441777&Signature=mMR0vRZRqJQvQmeJ4qYTh6xMkp0%3D",
                    ImageUrl = "https://hcm.ss.bfcplatform.vn/zengarden/male.png?AWSAccessKeyId=8QEUOTPT6CM3J3X9CD9T&Expires=1743441822&Signature=DJNiWS8ebIWoyCqDEqfccJocY5I%3D",
                    CreatedAt = DateTime.UtcNow
                };

                // Create all entities in parallel
                var createTasks = new[]
                {
                    walletRepository.CreateAsync(wallet),
                    bagRepository.CreateAsync(bag),
                    userExperienceRepository.CreateAsync(userExperience),
                    userConfigRepository.CreateAsync(userConfig)
                };

                await Task.WhenAll(createTasks);
                await unitOfWork.CommitAsync();
                
                return state;
            },
            null,
            CancellationToken.None
        );
    }

    public async Task<string> GenerateAndSaveOtpAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email)
                   ?? throw new KeyNotFoundException("User not found.");

        var otp = new Random().Next(100000, 999999).ToString();

        user.OtpCodeHash = PasswordHasher.HashPassword(otp);
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await unitOfWork.CommitAsync();
        return otp;
    }

    public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user?.OtpCodeHash == null || user.OtpExpiry < DateTime.UtcNow) return false;

        var isValid = PasswordHasher.VerifyPassword(otp, user.OtpCodeHash);
        if (!isValid) return false;

        user.Password = PasswordHasher.HashPassword(newPassword);
        user.OtpCodeHash = null;
        user.OtpExpiry = null;

        await unitOfWork.CommitAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!PasswordHasher.VerifyPassword(oldPassword, user.Password)) return false;

        user.Password = PasswordHasher.HashPassword(newPassword);
        await unitOfWork.CommitAsync();
        return true;
    }

    public async Task OnUserLoginAsync(int userId)
    {
        using var scope = scopeFactory.CreateScope();
        var userTreeRepository = scope.ServiceProvider.GetRequiredService<IUserTreeRepository>();
        var userTreeService = scope.ServiceProvider.GetRequiredService<IUserTreeService>();

        // Get all user trees in one query
        var userTrees = await userTreeRepository.GetUserTreeByUserIdAsync(userId);
            
        // Create tasks for parallel processing
        var tasks = new List<Task>();
            
        // Add tree health update tasks
        foreach (var tree in userTrees)
        {
            tasks.Add(userTreeService.UpdateSpecificTreeHealthAsync(tree.UserTreeId));
        }
            
        // Execute all tasks in parallel
        await Task.WhenAll(tasks);
    }

    public async Task MakeItemdefault(int userid)
    {
        if (userid == 0)
            throw new ArgumentException("User Id cannot be empty.");

        var bag = await bagRepository.GetByUserIdAsync(userid)
                  ?? throw new KeyNotFoundException($"Bag for user with ID {userid} not found.");

        var itembag = new BagItem
        {
            BagId = bag.BagId,
            ItemId = 1,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow
        };

        await bagItemRepository.CreateAsync(itembag);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create item in bag.");
    }

    public async Task<List<Users>> GetAllUserFilterAsync(UserFilterDto filter)
    {
        var userFilterResult = await userRepository.GetAllAsync(filter);
        if (userFilterResult.Data == null)
            throw new KeyNotFoundException("User not found.");
        return userFilterResult.Data;
    }

    public async Task CreateUserAsync(Users user)
    {
        var existingUser = await userRepository.GetByEmailAsync(user.Email);
        if (existingUser != null)
            throw new InvalidOperationException($"User with email {user.Email} already exists.");

        await userRepository.CreateAsync(user);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create user.");
    }

    private static string GenerateRandomUsername()
    {
        return $"User_{Guid.NewGuid().ToString("N")[..8]}";
    }
}