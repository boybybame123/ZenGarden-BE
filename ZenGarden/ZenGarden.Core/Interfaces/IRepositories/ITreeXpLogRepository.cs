using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITreeXpLogRepository : IGenericRepository<TreeXpLog>
{
    Task<List<TreeXpLog>> GetTreeXpLogByTaskIdAsync(int taskId);
    Task<TreeXpLog?> GetLatestTreeXpLogByUserTreeIdAsync(int userTreeId);
}