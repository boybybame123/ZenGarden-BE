using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
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
    ITreeXpLogRepository treeXpLogRepository,
    IUserTreeService userTreeService,
    IFocusMethodService focusMethodService,
    IXpConfigService xpConfigService,
    IUserChallengeRepository userChallengeRepository,
    IChallengeTaskRepository challengeTaskRepository,
    IS3Service s3Service,
    IMapper mapper,
    IValidator<CreateTaskDto> createTaskValidator) : ITaskService
{
    public async Task<List<TaskDto>> GetAllTaskAsync()
    {
        var tasks = await taskRepository.GetAllWithDetailsAsync();
        return mapper.Map<List<TaskDto>>(tasks);
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int taskId)
    {
        var task = await taskRepository.GetTaskWithDetailsAsync(taskId);
        if (task == null) throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var taskDto = mapper.Map<TaskDto>(task);

        switch (task.Status)
        {
            case TasksStatus.InProgress when task.StartedAt != null:
            {
                var elapsedTime = (DateTime.UtcNow - task.StartedAt.Value).TotalMinutes;
                var remainingTime = (task.TotalDuration ?? 0) - (int)elapsedTime;
                taskDto.RemainingTime = Math.Max(remainingTime, 0);
                break;
            }
            case TasksStatus.Paused:
            {
                if (task is { PausedAt: not null, StartedAt: not null })
                {
                    var elapsedTime = (task.PausedAt.Value - task.StartedAt.Value).TotalMinutes;
                    var remainingTime = (task.TotalDuration ?? 0) - (int)elapsedTime;
                    taskDto.RemainingTime = Math.Max(remainingTime, 0);
                }
                else
                {
                    taskDto.RemainingTime = task.TotalDuration ?? 0;
                }

                break;
            }
            case TasksStatus.NotStarted:
            case TasksStatus.Completed:
            case TasksStatus.Overdue:
            case TasksStatus.Canceled:
            default:
                taskDto.RemainingTime = task.TotalDuration ?? 0;
                break;
        }

        return taskDto;
    }


    public async Task<List<TaskDto>> GetTaskByUserIdAsync(int userId)
    {
        var tasks = await taskRepository.GetTasksByUserIdAsync(userId);

        if (tasks == null || tasks.Count == 0)
            throw new KeyNotFoundException($"Tasks with UserTree ID {userId} not found.");

        return mapper.Map<List<TaskDto>>(tasks);
    }

    public async Task<List<TaskDto>> GetTaskByUserTreeIdAsync(int userTreeId)
    {
        var tasks = await taskRepository.GetTasksByUserTreeIdAsync(userTreeId);

        if (tasks == null || tasks.Count == 0)
            throw new KeyNotFoundException($"Tasks with UserTree ID {userTreeId} not found.");

        return mapper.Map<List<TaskDto>>(tasks);
    }

    public async Task<TaskDto> CreateTaskWithSuggestedMethodAsync(CreateTaskDto dto)
    {
        await ValidateTaskDto(dto);
        var selectedMethod = await GetFocusMethodAsync(dto);

        await xpConfigService.EnsureXpConfigExists(
            selectedMethod.FocusMethodId,
            dto.TaskTypeId,
            dto.TotalDuration ?? 30
        );

        var newTask = new Tasks
        {
            TaskTypeId = dto.TaskTypeId,
            UserTreeId = dto.UserTreeId,
            FocusMethodId = selectedMethod.FocusMethodId,
            TaskName = dto.TaskName,
            TaskDescription = dto.TaskDescription,
            TotalDuration = dto.TotalDuration,
            WorkDuration = selectedMethod.DefaultDuration ?? 25,
            BreakTime = selectedMethod.DefaultBreak ?? 5,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedAt = DateTime.UtcNow,
            Status = TasksStatus.NotStarted,
            IsSuggested = dto.WorkDuration == null && dto.BreakTime == null
        };

        await taskRepository.CreateAsync(newTask);
        await unitOfWork.CommitAsync();
        return mapper.Map<TaskDto>(newTask);
    }

    public async Task UpdateTaskAsync(UpdateTaskDto updateTaskDto)
    {
        var existingTask = await taskRepository.GetByIdAsync(updateTaskDto.TaskId)
                           ?? throw new KeyNotFoundException($"Task with ID {updateTaskDto.TaskId} not found.");

        if (existingTask.Status is not (TasksStatus.InProgress or TasksStatus.Paused))
            throw new InvalidOperationException("Only tasks in 'InProgress' or 'Paused' state can be completed.");

        mapper.Map(updateTaskDto, existingTask);

        var updatedTaskResult = await HandleTaskResultUpdate(updateTaskDto.TaskFile, updateTaskDto.TaskResult);
        if (!string.IsNullOrWhiteSpace(updatedTaskResult))
            existingTask.TaskResult = updatedTaskResult;

        if (updateTaskDto.FocusMethodId.HasValue)
        {
            _ = await focusMethodRepository.GetByIdAsync(updateTaskDto.FocusMethodId.Value)
                ?? throw new KeyNotFoundException(
                    $"FocusMethod with ID {updateTaskDto.FocusMethodId.Value} not found.");

            existingTask.FocusMethodId = updateTaskDto.FocusMethodId.Value;
        }

        existingTask.UpdatedAt = DateTime.UtcNow;
        taskRepository.Update(existingTask);

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update task.");
    }


    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await taskRepository.GetByIdAsync(taskId);
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

        if (task.Status == TasksStatus.InProgress)
            throw new InvalidOperationException("Task is already in progress.");

        var existingInProgressTask = await taskRepository.GetUserTaskInProgressAsync(userId);
        if (existingInProgressTask != null && existingInProgressTask.TaskId != taskId)
            throw new InvalidOperationException(
                $"You already have a task in progress (Task ID: {existingInProgressTask.TaskId}). Please complete it first.");

        if (task.Status == TasksStatus.NotStarted)
        {
            task.StartedAt = DateTime.UtcNow;
            task.Status = TasksStatus.InProgress;
        }
        else if (task is { Status: TasksStatus.Paused, PausedAt: not null, StartedAt: not null })
        {
            var elapsedTime = (task.PausedAt.Value - task.StartedAt.Value).TotalMinutes;
            var remainingTime = (task.TotalDuration ?? 0) - (int)elapsedTime;

            if (remainingTime <= 0)
                throw new InvalidOperationException("Task has already expired.");

            task.StartedAt = DateTime.UtcNow;
            task.Status = TasksStatus.InProgress;
        }
        else
        {
            throw new InvalidOperationException("Task cannot be started from the current state.");
        }

        taskRepository.Update(task);

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to start the task.");
    }

    public async Task CompleteTaskAsync(int taskId)
    {
        var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        ValidateTaskForCompletion(task);

        if (task.FocusMethodId != null)
        {
            var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                           ?? throw new KeyNotFoundException("XP configuration not found for this task.");

            var xpEarned = xpConfig.BaseXp * xpConfig.XpMultiplier;

            await using var transaction = await unitOfWork.BeginTransactionAsync();
            try
            {
                if (xpEarned > 0)
                {
                    task.UserTree.TotalXp += xpEarned;

                    await treeXpLogRepository.CreateAsync(new TreeXpLog
                    {
                        TaskId = task.TaskId,
                        ActivityType = ActivityType.TaskXp,
                        XpAmount = xpEarned,
                        CreatedAt = DateTime.UtcNow
                    });
                    await userTreeService.CheckAndSetMaxLevelAsync(task.UserTree);
                }

                task.Status = TasksStatus.Completed;
                task.CompletedAt = DateTime.UtcNow;

                taskRepository.Update(task);
                userTreeRepository.Update(task.UserTree);


                await unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        var challengeTask = await challengeTaskRepository.GetByTaskIdAsync(task.TaskId);
        if (challengeTask != null)
        {
            if (task.UserTree?.UserId == null)
                throw new InvalidOperationException("Task is not associated with a valid UserTree.");

            await userChallengeRepository.UpdateProgressAsync(task.UserTree.UserId.Value, challengeTask.ChallengeId);
        }
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

    public async Task<double> CalculateTaskXpAsync(int taskId)
    {
        var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                   ?? throw new KeyNotFoundException("Task not found.");

        if (task.FocusMethodId == null)
            throw new InvalidOperationException("Task does not have an assigned FocusMethod.");

        var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                       ?? throw new KeyNotFoundException("XP configuration not found.");

        if (xpConfig.BaseXp <= 0)
            throw new InvalidOperationException("Invalid BaseXp value in XP configuration.");

        return xpConfig.BaseXp * xpConfig.XpMultiplier;
    }

    public async Task PauseTaskAsync(int taskId)
    {
        var task = await taskRepository.GetByIdAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (task.Status != TasksStatus.InProgress)
            throw new InvalidOperationException("Only in-progress tasks can be paused.");

        if (task.StartedAt != null)
        {
            var elapsedTime = (DateTime.UtcNow - task.StartedAt.Value).TotalMinutes;
            var remainingTime = (task.TotalDuration ?? 0) - (int)elapsedTime;

            if (remainingTime <= 0)
                throw new InvalidOperationException("Task has already exceeded its duration.");
        }

        task.PausedAt = DateTime.UtcNow;
        task.Status = TasksStatus.Paused;

        taskRepository.Update(task);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to pause the task.");
    }

    public async Task AutoPauseTasksAsync()
    {
        var thresholdTime = DateTime.UtcNow.AddMinutes(-10);
        if (thresholdTime == default) throw new ArgumentException("Threshold time cannot be default or null.");

        var inProgressTasks = await taskRepository.GetTasksInProgressBeforeAsync(thresholdTime);
        foreach (var task in inProgressTasks)
        {
            task.Status = TasksStatus.Paused;
            task.PausedAt = DateTime.UtcNow;
            taskRepository.Update(task);
        }

        if (inProgressTasks.Count > 0)
            await unitOfWork.CommitAsync();
    }

    public async Task<string> HandleTaskResultUpdate(IFormFile? taskResultFile, string? taskResultUrl)
    {
        if (taskResultFile != null)
            return await s3Service.UploadFileAsync(taskResultFile);

        if (string.IsNullOrWhiteSpace(taskResultUrl)) return string.Empty;
        if (Uri.IsWellFormedUriString(taskResultUrl, UriKind.Absolute))
            return taskResultUrl;
        throw new ArgumentException("Invalid TaskResult URL.");
    }


    public async Task ValidateTaskDto(CreateTaskDto dto)
    {
        var validationResult = await createTaskValidator.ValidateAsync(dto);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);
        var taskType = await taskTypeRepository.GetByIdAsync(dto.TaskTypeId);
        if (taskType == null)
            throw new KeyNotFoundException($"TaskType not found. TaskTypeId = {dto.TaskTypeId}");

        if (dto.UserTreeId is > 0)
        {
            var userTree = await userTreeRepository.GetByIdAsync(dto.UserTreeId.Value);
            if (userTree == null)
                throw new KeyNotFoundException($"UserTree not found. UserTreeId = {dto.UserTreeId}");
        }
    }


    private async Task<FocusMethodDto> GetFocusMethodAsync(CreateTaskDto dto)
    {
        if (dto.FocusMethodId.HasValue)
        {
            var method = await focusMethodRepository.GetDtoByIdAsync(dto.FocusMethodId.Value);
            if (method != null) return method;
        }

        var suggestedMethod = await focusMethodService.SuggestFocusMethodAsync(new SuggestFocusMethodDto
        {
            TaskName = dto.TaskName,
            TaskDescription = dto.TaskDescription,
            TotalDuration = dto.TotalDuration,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        });

        return suggestedMethod ?? throw new InvalidOperationException("No valid focus method found.");
    }

    private static void ValidateTaskForCompletion(Tasks task)
    {
        if (task.Status != TasksStatus.InProgress && task.Status != TasksStatus.Paused)
            throw new InvalidOperationException(
                $"Only 'In Progress' or 'Paused' tasks can be completed. Current status: {task.Status}.");

        if (task.UserTree == null)
            throw new InvalidOperationException("Task is not linked to any UserTree.");

        if (task.FocusMethodId == null)
            throw new InvalidOperationException("Task does not have an assigned FocusMethod.");

        if (task.StartedAt == null)
            throw new InvalidOperationException("Task must have a start time to be completed.");

        if (task.TotalDuration.HasValue &&
            DateTime.UtcNow - task.StartedAt < TimeSpan.FromMinutes(task.TotalDuration.Value))
            throw new InvalidOperationException("Task cannot be completed before the required duration has passed.");
    }
}