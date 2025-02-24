using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserRepository : IGenericRepository<Users>
{ 
    Task<Users?> ValidateUserAsync(string? email, string? phone, string password);
    Task<Users?> GetByEmailAsync(string email);
    Task<Users?> GetUserByRefreshTokenAsync(string refreshToken);
    Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate);
    Task<Roles?> GetRoleByIdAsync(int roleId);
    Task<Users?> GetByPhoneAsync(string phone);
}