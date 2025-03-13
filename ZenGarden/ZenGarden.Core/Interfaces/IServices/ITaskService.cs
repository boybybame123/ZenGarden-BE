using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITaskService
{
    Task<List<Tasks>> GetAllTaskAsync();
    Task<Tasks?> GetTaskByIdAsync(int taskId);
    Task<Tasks> CreateTaskWithSuggestedMethodAsync(CreateTaskDto dto);
    Task UpdateTaskAsync(UpdateTaskDto updateTaskDto);
    Task DeleteTaskAsync(int taskId);
    Task StartTaskAsync(int taskId, int userId);
    Task CompleteTaskAsync(int taskId);
    Task UpdateOverdueTasksAsync();
}