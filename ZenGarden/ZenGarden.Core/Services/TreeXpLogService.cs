using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TreeXpLogService : ITreeXpLogService
{
    private readonly ITreeXpLogRepository _treeXpLogRepository;

    public TreeXpLogService(ITreeXpLogRepository treeXpLogRepository)
    {
        _treeXpLogRepository = treeXpLogRepository;
    }

    public async Task<List<TreeXpLog>> GetTreeXpLogByTaskIdAsync(int taskId)
    {
        return await _treeXpLogRepository.GetTreeXpLogByTaskIdAsync(taskId);
    }

    public async Task<TreeXpLog?> GetTreeXpLogByLogIdAsync(int logId)
    {
        return await _treeXpLogRepository.GetByIdAsync(logId);
    }

    public async Task<List<TreeXpLog>> GetAllTreeXpLog()
    {
        return await _treeXpLogRepository.GetAllAsync();
    }

    public async Task<TreeXpLog?> GetLatestTreeXpLogByUserTreeIdAsync(int userTreeId)
    {
        return await _treeXpLogRepository.GetLatestTreeXpLogByUserTreeIdAsync(userTreeId);
    }

    public async Task<List<TreeXpLog>> GetTreeXpLogByUserIdAsync(int userId)
    {
        return await _treeXpLogRepository.GetTreeXpLogByUserIdAsync(userId);
    }

    public async Task<List<TreeXpLog>> GetTreeXpLogByUserTreeIdAsync(int userTreeId)
    {
        return await _treeXpLogRepository.GetTreeXpLogByUserTreeIdAsync(userTreeId);
    }
}