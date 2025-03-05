using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Shared.Helpers;

namespace ZenGarden.Core.Services;

public class UserService(
    IUserRepository userRepository,
    IBagRepository bagRepository,
    IWalletRepository walletRepository,
    IUserLevelConfigRepository userLevelConfigRepository,
    IUserExperienceRepository userExperienceRepository,
    IUnitOfWork unitOfWork,
    IWorkspaceRepository workspaceRepository,
    IMapper mapper) : IUserService
{
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return mapper.Map<List<UserDto>>(users);
    }

    public async Task<List<Users>> GetAllUserFilterAsync(UserFilterDto filter)
    {
        var userFilterResult = await userRepository.GetAllAsync(filter);
        if (userFilterResult.Data == null)
            throw new KeyNotFoundException("User not found.");
        return userFilterResult.Data;
    }

    public async Task<Users?> GetUserByIdAsync(int userId)
    {
        return await userRepository.GetByIdAsync(userId)
               ?? throw new KeyNotFoundException($"User with ID {userId} not found.");
    }

    public async Task ChangeUserisActiveAsync(int userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            userRepository.Update(user);
        }

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update user.");
    }


    public async Task<Users?> GetUserByEmailAsync(string email)
    {
        return await userRepository.GetByEmailAsync(email);
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


    public async Task UpdateUserAsync(UserDto user)
    {
        var userUpdate = await GetUserByIdAsync(user.UserId);
        if (userUpdate == null)
            throw new KeyNotFoundException($"User with ID {user.UserId} not found.");
        mapper.Map(user, userUpdate);
        userRepository.Update(userUpdate);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update user.");
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
        var user = !string.IsNullOrEmpty(email)
            ? await userRepository.GetByEmailAsync(email)
            : await userRepository.GetByPhoneAsync(phone);

        if (user == null || string.IsNullOrEmpty(user.Password)) return null;

        return PasswordHasher.VerifyPassword(password, user.Password) ? user : null;
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

        var existingUser = await userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null) throw new InvalidOperationException("Email is already in use.");

        var role = await userRepository.GetRoleByIdAsync(dto.RoleId ?? 2)
                   ?? throw new InvalidOperationException("Invalid RoleId.");

        var newUser = mapper.Map<Users>(dto);

        newUser.UserName = string.IsNullOrWhiteSpace(dto.FullName) ? GenerateRandomUsername() : dto.FullName;

        newUser.Password = PasswordHasher.HashPassword(dto.Password);
        newUser.RoleId = role.RoleId;
        newUser.Status = UserStatus.Active;
        newUser.IsActive = true;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            await userRepository.CreateAsync(newUser);
            await unitOfWork.CommitAsync();

            var wallet = new Wallet { UserId = newUser.UserId, Balance = 0 };
            var bag = new Bag { UserId = newUser.UserId };
            var workspace = new Workspace
            {
                UserId = newUser.UserId,
                Configuration =
                    "{\"theme\": \"light\", \"notifications\": true, \"layout\": \"grid\", \"widgets\": []}",
                CreatedAt = DateTime.UtcNow
            };

            var levelOneConfig = await userLevelConfigRepository.GetByIdAsync(1)
                                 ?? throw new Exception("UserLevelConfig is missing Level 1!");

            var userExperience = new UserExperience
            {
                UserId = newUser.UserId,
                TotalXp = 0,
                CurrentLevel = 1,
                XpToNextLevel = levelOneConfig.XpRequired,
                LevelId = 1,
                UpdatedAt = DateTime.UtcNow
            };

            await walletRepository.CreateAsync(wallet);
            await bagRepository.CreateAsync(bag);
            await workspaceRepository.CreateAsync(workspace);
            await userExperienceRepository.CreateAsync(userExperience);

            await unitOfWork.CommitAsync();
            await unitOfWork.CommitTransactionAsync();
            return newUser;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
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


    private static string GenerateRandomUsername()
    {
        return $"User_{Guid.NewGuid().ToString("N")[..8]}";
    }
}