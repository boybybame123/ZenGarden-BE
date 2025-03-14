using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITaskRepository : IGenericRepository<Tasks>
{
    Task<Tasks?> GetUserTaskInProgressAsync(int userId);
    Task<Tasks?> GetTaskWithDetailsAsync(int taskId);
    Task<List<Tasks>> GetOverdueTasksAsync();
}