using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllTaskAsync();
    Task<TaskDto?> GetTaskByIdAsync(int taskId);
    Task<TaskDto> CreateTaskWithSuggestedMethodAsync(CreateTaskDto dto);
    Task UpdateTaskAsync(UpdateTaskDto updateTaskDto);
    Task DeleteTaskAsync(int taskId);
    Task StartTaskAsync(int taskId, int userId);
    Task UpdateOverdueTasksAsync();
    Task<double> CalculateTaskXpAsync(int taskId);
    Task<List<TaskDto>> GetTaskByUserTreeIdAsync(int userTreeId);
    Task<List<TaskDto>> GetTaskByUserIdAsync(int userId);
    Task PauseTaskAsync(int taskId);
    Task AutoPauseTasksAsync();
    Task ResetDailyTasksAsync();
    Task CompleteTaskAsync(int taskId, CompleteTaskDto completeTaskDto);
}