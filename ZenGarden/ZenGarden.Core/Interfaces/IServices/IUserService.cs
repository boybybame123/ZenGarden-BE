using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<List<Users>> GetAllUserFilterAsync(UserFilterDto filter);
    Task<Users?> GetUserByIdAsync(int userId);
    Task<Users?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(Users user);
    Task UpdateUserAsync(Users user);
    Task DeleteUserAsync(int userId);
    Task<Users?> ValidateUserAsync(string? email, string? phone, string password);
    Task<Users?> GetUserByRefreshTokenAsync(string refreshToken);
    Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate);
    Task RemoveRefreshTokenAsync(int userUserId);
    Task<Users?> RegisterUserAsync(RegisterDto dto);
    Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);
    Task<string> GenerateAndSaveOtpAsync(string email);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}