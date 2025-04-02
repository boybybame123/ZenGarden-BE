using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserXpConfigService
{
    Task<List<UserXpConfigDto>> GetAllUserXpConfigsAsync();
    Task<UserXpConfig?> GetUserXpConfigByIdAsync(int userXpConfigId);
    Task CreateUserXpConfigAsync(UserXpConfigDto userXpConfig);
    Task UpdateUserXpConfigAsync(UserXpConfigDto userXpConfig);
    Task DeleteUserXpConfigAsync(int userXpConfigId);
}