using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserXpConfigRepository : IGenericRepository<UserXpConfig>
{
    Task<int> GetXpThresholdForLevel(int levelId);
    Task<UserXpConfig> GetMaxLevelConfig();
}