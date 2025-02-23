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
}