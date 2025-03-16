using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITreeXpConfigRepository : IGenericRepository<TreeXpConfig>
{
    Task<int> GetMaxXpThresholdAsync();
}