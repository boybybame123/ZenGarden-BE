using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TreeXpLogService(ITreeXpLogRepository treeXpLogRepository) : ITreeXpLogService
{
    public async Task<List<TreeXpLog>> GetTreeXpLogByTaskIdAsync(int taskId)
    {
        return await treeXpLogRepository.GetTreeXpLogByTaskIdAsync(taskId);
    }

    public async Task<TreeXpLog?> GetTreeXpLogByLogIdAsync(int logId)
    {
        return await treeXpLogRepository.GetByIdAsync(logId);
    }

    public async Task<List<TreeXpLog>> GetAllTreeXpLog()
    {
        return await treeXpLogRepository.GetAllAsync();
    }
}