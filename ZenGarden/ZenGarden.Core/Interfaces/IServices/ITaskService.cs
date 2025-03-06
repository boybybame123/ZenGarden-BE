using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllTaskAsync();
    Task<Tasks?> GetTaskByIdAsync(int taskId);
    Task<Tasks> CreateTaskAsync(CreateTaskDto dto);
    Task UpdateTaskAsync(TaskDto task);
    Task DeleteTaskAsync(int taskId);
}