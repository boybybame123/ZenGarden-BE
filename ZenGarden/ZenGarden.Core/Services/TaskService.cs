using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class TaskService(
    ITaskRepository taskRepository,
    IFocusMethodRepository focusMethodRepository,
    IUnitOfWork unitOfWork,
    IUserTreeRepository userTreeRepository,
    IXpConfigRepository xpConfigRepository,
    ITaskTypeRepository taskTypeRepository,
    IMapper mapper) : ITaskService
{
    public async Task<List<Tasks>> GetAllTaskAsync()
    {
        return await taskRepository.GetAllAsync(); 
    }

    public async Task<Tasks?> GetTaskByIdAsync(int taskId)
    {
        return await taskRepository.GetByIdAsync(taskId)
               ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");
    }
    

    public async Task<Tasks> CreateTaskWithSuggestedMethodAsync(CreateTaskDto dto)
    {
        if (dto.FocusMethodId == null)
            throw new ArgumentException("FocusMethodId is required. Please call SuggestFocusMethod first.");

        var existingMethod = await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value);
        if (existingMethod == null)
            throw new KeyNotFoundException("FocusMethod not found.");
        
        var existingTaskType = await taskTypeRepository.GetByIdAsync(dto.TaskTypeId);
        if (existingTaskType == null)
            throw new KeyNotFoundException("TaskType not found.");

        var workDuration = dto.WorkDuration ?? existingMethod.DefaultDuration ?? 25;
        var breakTime = dto.BreakTime ?? existingMethod.DefaultBreak ?? 5;

        var newTask = new Tasks
        {
            TaskTypeId = dto.TaskTypeId,
            UserTreeId = dto.UserTreeId,
            FocusMethodId = existingMethod.FocusMethodId,
            TaskName = dto.TaskName,
            TaskDescription = dto.TaskDescription,
            WorkDuration = workDuration,
            BreakTime = breakTime,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedAt = DateTime.UtcNow,
            Status = TasksStatus.NotStarted,
            IsSuggested = dto.WorkDuration == null && dto.BreakTime == null
        };

        await taskRepository.CreateAsync(newTask);
        return newTask;
    }

    
    public async Task UpdateTaskAsync(UpdateTaskDto updateTaskDto)
    {
        var existingTask = await GetTaskByIdAsync(updateTaskDto.TaskId);
        if (existingTask == null)
            throw new KeyNotFoundException($"Task with ID {updateTaskDto.TaskId} not found.");

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskName))
            existingTask.TaskName = updateTaskDto.TaskName;

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskDescription))
            existingTask.TaskDescription = updateTaskDto.TaskDescription;
        
        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskNote))
            existingTask.TaskNote = updateTaskDto.TaskNote;

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskResult))
            existingTask.TaskResult = updateTaskDto.TaskResult;

        if (updateTaskDto.WorkDuration.HasValue)
            existingTask.WorkDuration = updateTaskDto.WorkDuration.Value;

        if (updateTaskDto.BreakTime.HasValue)
            existingTask.BreakTime = updateTaskDto.BreakTime.Value;

        if (updateTaskDto.StartDate.HasValue)
            existingTask.StartDate = updateTaskDto.StartDate.Value;

        if (updateTaskDto.EndDate.HasValue)
            existingTask.EndDate = updateTaskDto.EndDate.Value;

        existingTask.UpdatedAt = DateTime.UtcNow;

        taskRepository.Update(existingTask);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update task.");
    }

    
    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await GetTaskByIdAsync(taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        await taskRepository.RemoveAsync(task);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete task.");
    }

    public async Task StartTaskAsync(int taskId, int userId)
    {
        var task = await taskRepository.GetByIdAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (task.Status != TasksStatus.NotStarted)
            throw new InvalidOperationException("Only not started tasks can be started.");

        var existingInProgressTask = await taskRepository.GetUserTaskInProgressAsync(userId);
        if (existingInProgressTask != null)
            throw new InvalidOperationException($"You already have a task in progress (Task ID: {existingInProgressTask.TaskId}). Please complete it first.");

        task.Status = TasksStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;
        taskRepository.Update(task);

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to start the task.");
    }


    public async Task CompleteTaskAsync(int taskId)
    {
        var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (task.Status != TasksStatus.InProgress)
            throw new InvalidOperationException("Only in-progress tasks can be completed.");

        if (task.UserTree == null)
            throw new InvalidOperationException("Task is not linked to any UserTree.");

        if (task.FocusMethodId == null)
            throw new InvalidOperationException("Task does not have an assigned FocusMethod.");
        var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                       ?? throw new KeyNotFoundException("XP configuration not found for this task.");

        var standardDuration = task.FocusMethod?.MinDuration ?? 25;
        var workDuration = (double)(task.WorkDuration ?? 25);
        var xpEarned = workDuration / standardDuration * xpConfig.BaseXp * xpConfig.Multiplier;

        task.UserTree.TotalXp += xpEarned;
        task.Status = TasksStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;

        taskRepository.Update(task);
        userTreeRepository.Update(task.UserTree);

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to complete task.");
    }
    public async Task UpdateOverdueTasksAsync()
    {
        var overdueTasks = await taskRepository.GetOverdueTasksAsync();

        foreach (var task in overdueTasks)
        {
            task.Status = TasksStatus.Overdue;
            task.UpdatedAt = DateTime.UtcNow;
        }

        if (overdueTasks.Count != 0)
            await unitOfWork.CommitAsync();
    }

}