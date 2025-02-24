using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserService
{
    Task<List<Users>> GetAllUsersAsync();
    Task<Users?> GetUserByIdAsync(int userId);
    Task CreateUserAsync(Users user);
    Task UpdateUserAsync(Users user);
    Task DeleteUserAsync(int userId);
    Task<Users?> ValidateUserAsync(string? email, string? phone, string password);
    Task<Users?> GetUserByRefreshTokenAsync(string refreshToken);
    Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate);
    Task RemoveRefreshTokenAsync(int userUserId);
    Task<Users?> RegisterUserAsync(RegisterDto dto);
}