using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface IUserXpConfigService
    {
        Task<List<UserXpConfigDto>> GetAllUserXpConfigsAsync();
        Task<UserXpConfig?> GetUserXpConfigByIdAsync(int UserXpConfigId);
        Task CreateUserXpConfigAsync(UserXpConfigDto UserXpConfig);
        Task UpdateUserXpConfigAsync(UserXpConfigDto UserXpConfig);
        Task DeleteUserXpConfigAsync(int UserXpConfigId);
    }
}
