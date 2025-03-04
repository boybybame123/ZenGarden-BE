using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllUsersAsync();
    Task<Tasks?> GetTaskByIdAsync(int TaskId);
    Task CreateTaskAsync(Tasks task);
    Task UpdateTaskAsync(TaskDto task);
    Task DeleteTaskAsync(int TaskId);


}