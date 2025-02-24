using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Shared.Helpers;

namespace ZenGarden.Core.Services
{
    public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserService
    {
        public async Task<List<Users>> GetAllUsersAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        public async Task CreateUserAsync(Users user)
        {
            _ = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null.");

            var existingUser = await userRepository.GetByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {user.Email} already exists.");
            }

            userRepository.Create(user);
            if (await unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to create user.");
        }


        public async Task UpdateUserAsync(Users user)
        {
            _ = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null.");
            
            userRepository.Update(user);
            if (await unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to update user.");
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await userRepository.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            userRepository.Remove(user);
            if (await unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to delete user.");
        }

        public async Task<Users?> ValidateUserAsync(string? email, string? phone, string password)
        {
            Users? user = null;
    
            if (!string.IsNullOrEmpty(email))
            {
                user = await userRepository.GetByEmailAsync(email);
            }
    
            if (user == null && !string.IsNullOrEmpty(phone))
            {
                user = await userRepository.GetByPhoneAsync(phone);
            }

            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                return null;
            }

            var isPasswordValid = PasswordHasher.VerifyPassword(password, user.Password);
            return isPasswordValid ? user : null;
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
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiry = DateTime.UtcNow; 

            await unitOfWork.CommitAsync();
        }


        public async Task<Users?> RegisterUserAsync(RegisterDto dto)
        {
            var existingUser = await userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already in use.");
            }

            var roleId = dto.RoleId ?? 2; 

            var role = await userRepository.GetRoleByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException("Invalid RoleId.");
            }

            var hashedPassword = PasswordHasher.HashPassword(dto.Password);

            var newUser = new Users
            {
                Email = dto.Email,
                Phone = dto.Phone,
                Password = hashedPassword,
                RoleId = role.RoleId,
                Role = role, 
                RefreshToken = string.Empty,
                RefreshTokenExpiry = DateTime.UtcNow 
            };

            userRepository.Create(newUser);
            var result = await unitOfWork.CommitAsync();
            if (result == 0)
            {
                throw new InvalidOperationException("Failed to create user.");
            }

            return newUser;
        }
    }
}