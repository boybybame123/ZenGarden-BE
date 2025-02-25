using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserRepository : IGenericRepository<Users>
{
    Task<Users?> GetByEmailAsync(string email);
    Task<Users?> GetUserByRefreshTokenAsync(string refreshToken);
    Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate);
    Task<Roles?> GetRoleByIdAsync(int roleId);
    Task<Users?> GetByPhoneAsync(string phone);
    Task<FilterResult<Users>> GetAllAsync(UserFilterDto filter);
}