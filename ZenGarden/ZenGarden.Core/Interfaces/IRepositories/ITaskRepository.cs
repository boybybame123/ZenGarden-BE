using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITaskRepository : IGenericRepository<Tasks>
{
    Task<Tasks?> GetUserTaskInProgressAsync(int userId);
    Task<Tasks?> GetTaskWithDetailsAsync(int taskId);
    Task<List<Tasks>> GetOverdueTasksAsync();
    Task<List<Tasks>> GetAllWithDetailsAsync();
    Task<List<Tasks>> GetTasksByUserTreeIdAsync(int userTreeId);
    Task<List<Tasks>> GetTasksByUserIdAsync(int userId);
    Task<List<Tasks>> GetClonedTasksByUserChallengeAsync(int userId, int challengeId);
    Task<List<Tasks>> GetAllTasksByChallengeIdAsync(int challengeId);
    Task<List<Tasks>> GetTasksInProgressBeforeAsync(DateTime thresholdTime);
    Task<Tasks?> GetActiveTaskByUserTreeIdAsync(int userTreeId);
    Task<List<Tasks>> GetDailyTasksAsync();
    Task<int> GetCompletedTasksAsync(int userId, int challengeId);
    Task<int> GetTotalCloneTasksAsync(int userId, int challengeId);
    Task<int> GetNextPriorityForTreeAsync(int userTreeId);
    Task<List<Tasks>> GetTasksByIdsAsync(List<int> taskIds);
    Task<List<Tasks>> GetActiveTasksByUserTreeIdAsync(int userTreeId);
    Task<int?> GetUserIdByTaskIdAsync(int taskId);
}