using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllTaskAsync();
    Task<TaskDto?> GetTaskByIdAsync(int taskId);
    Task<TaskDto> CreateTaskWithSuggestedMethodAsync(CreateTaskDto dto);
    Task<List<TaskDto>> CreateMultipleTasksWithSuggestedMethodAsync(List<CreateTaskDto> dtos);
    Task UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto);
    Task DeleteTaskAsync(int taskId);
    Task StartTaskAsync(int taskId, int userId);
    Task UpdateOverdueTasksAsync();
    Task<double> CalculateTaskXpAsync(int taskId);
    Task<TaskXpInfoDto> GetTaskXpInfoAsync(int taskId);
    Task<List<TaskDto>> GetTaskByUserTreeIdAsync(int userTreeId);
    Task<List<TaskDto>> GetTaskByUserIdAsync(int userId);
    Task<List<TaskDto>> GetClonedTasksByUserChallengeAsync(int userId, int challengeId);
    Task PauseTaskAsync(int taskId);
    Task AutoPauseTasksAsync();
    Task ResetDailyTasksAsync();
    Task<double> CompleteTaskAsync(int taskId, CompleteTaskDto completeTaskDto);
    Task ReorderTasksAsync(int userTreeId, List<ReorderTaskDto> reorderList);
    Task WeeklyTaskPriorityResetAsync();
    Task ForceUpdateTaskStatusAsync(int taskId, TasksStatus newStatus);
    Task UpdateTaskTypeAsync(int taskId, int newTaskTypeId, int newDuration);
    Task<List<Tasks>> GetTasksToNotifyAsync(DateTime currentTime);
    Task UpdateTaskSimpleAsync(int taskId, UpdateTaskSimpleDto updateTaskDto);
    Task UpdateTaskResultAsync(int taskId, UpdateTaskResultDto updateTaskResultDto);
}