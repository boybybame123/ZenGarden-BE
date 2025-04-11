using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Shared.Helpers;

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
    IUserChallengeService userChallengeService,
    IChallengeTaskRepository challengeTaskRepository,
    IS3Service s3Service,
    IMapper mapper,
    IBagRepository bagRepository,
    IUseItemService useItemService,
    IValidator<CreateTaskDto> createTaskValidator) : ITaskService
{
    public async Task<List<TaskDto>> GetAllTaskAsync()
    {
        var tasks = await taskRepository.GetAllWithDetailsAsync();
        var taskDto = mapper.Map<List<TaskDto>>(tasks);

        foreach (var (dto, entity) in taskDto.Zip(tasks))
        {
            var accumulatedSeconds = (int)((entity.AccumulatedTime ?? 0) * 60);
            var remainingSeconds = CalculateRemainingSeconds(entity);

            dto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);
            dto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
        }

        return taskDto;
    }


    public async Task<TaskDto?> GetTaskByIdAsync(int taskId)
    {
        var task = await taskRepository.GetTaskWithDetailsAsync(taskId);
        if (task == null) throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var taskDto = mapper.Map<TaskDto>(task);
        var remaining = CalculateRemainingSeconds(task);
        taskDto.RemainingTime = StringHelper.FormatSecondsToTime(remaining);
        var accumulatedSeconds = (int)((task.AccumulatedTime ?? 0) * 60);
        taskDto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);

        return taskDto;
    }

    public async Task<List<TaskDto>> GetTaskByUserIdAsync(int userId)
    {
        var tasks = await taskRepository.GetTasksByUserIdAsync(userId);
        if (tasks == null || tasks.Count == 0)
            throw new KeyNotFoundException($"Tasks with User ID {userId} not found.");

        var taskDto = mapper.Map<List<TaskDto>>(tasks);
        foreach (var (dto, entity) in taskDto.Zip(tasks))
        {
            var accumulatedSeconds = (int)((entity.AccumulatedTime ?? 0) * 60);
            var remainingSeconds = CalculateRemainingSeconds(entity);

            dto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);
            dto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
        }

        return taskDto;
    }

    public async Task<List<TaskDto>> GetTaskByUserTreeIdAsync(int userTreeId)
    {
        var tasks = await taskRepository.GetTasksByUserTreeIdAsync(userTreeId);
        if (tasks == null || tasks.Count == 0)
            throw new KeyNotFoundException($"Tasks with UserTree ID {userTreeId} not found.");

        var taskDto = mapper.Map<List<TaskDto>>(tasks);
        foreach (var (dto, entity) in taskDto.Zip(tasks))
        {
            var accumulatedSeconds = (int)((entity.AccumulatedTime ?? 0) * 60);
            var remainingSeconds = CalculateRemainingSeconds(entity);

            dto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);
            dto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
        }

        return taskDto;
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
            WorkDuration = dto.WorkDuration ?? selectedMethod.DefaultDuration ?? 25,
            BreakTime = dto.BreakTime ?? selectedMethod.DefaultBreak ?? 5,
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

    public async Task UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto)
    {
        var existingTask = await taskRepository.GetByIdAsync(taskId)
                           ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (existingTask.Status is (TasksStatus.InProgress or TasksStatus.Paused))
            throw new InvalidOperationException(
                "Tasks in 'InProgress' or 'Paused' state cannot be updated based on the new requirement.");

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

        var now = DateTime.UtcNow;
        if (now < task.StartDate)
            throw new InvalidOperationException("Task has not started yet.");
        if (now > task.EndDate)
            throw new InvalidOperationException("Task deadline has passed.");
        
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
            var elapsedBeforePause = (task.PausedAt.Value - task.StartedAt.Value).TotalMinutes;
            var totalAccumulated = task.AccumulatedTime + (int)elapsedBeforePause;

            if (totalAccumulated >= task.TotalDuration)
                throw new InvalidOperationException("Task has already expired.");

            task.AccumulatedTime = totalAccumulated;

            task.StartedAt = DateTime.UtcNow;
            task.PausedAt = null;
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

    public async Task<double> CompleteTaskAsync(int taskId, CompleteTaskDto completeTaskDto)
{
    double xpEarned = 0;

    var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
               ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

    if (await IsDailyTaskAlreadyCompleted(task))
        throw new InvalidOperationException("You have already completed this daily task today.");

    await UpdateUserTreeIfNeeded(task, completeTaskDto);

    ValidateTaskForCompletion(task);

    if (task.FocusMethodId != null)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                           ?? throw new KeyNotFoundException("XP configuration not found for this task.");

            var baseXp = xpConfig.BaseXp * xpConfig.XpMultiplier;
            
            xpEarned = await CalculateXpWithBoostAsync(task, baseXp);

            if (xpEarned > 0 && task.UserTree != null)
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

            task.CompletedAt = DateTime.UtcNow;
            task.Status = TasksStatus.Completed;

            taskRepository.Update(task);
            if (task.UserTree != null) userTreeRepository.Update(task.UserTree);

            await unitOfWork.CommitAsync();
            await transaction.CommitAsync();

            await UpdateChallengeProgress(task);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"Failed to complete task: {ex.Message}", ex);
        }
    }
    else
    {
        task.CompletedAt = DateTime.UtcNow;
        task.Status = TasksStatus.Completed;

        taskRepository.Update(task);
        await unitOfWork.CommitAsync();

        await UpdateChallengeProgress(task);
    }

    return xpEarned;
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

        if (task.StartedAt == null)
            throw new InvalidOperationException("Task has no start time.");

        var now = DateTime.UtcNow;

        var delta = (now - task.StartedAt.Value).TotalMinutes;

        task.AccumulatedTime = (task.AccumulatedTime ?? 0) + delta;

        var remainingTime = (task.TotalDuration ?? 0) - (int)task.AccumulatedTime.Value;
        if (remainingTime <= 0)
            throw new InvalidOperationException("Task has already exceeded its duration.");

        task.PausedAt = now;
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

    public async Task ResetDailyTasksAsync()
    {
        var dailyTasks = await taskRepository.GetDailyTasksAsync();
        foreach (var task in dailyTasks)
        {
            task.CompletedAt = null;
            task.Status = TasksStatus.NotStarted;
        }

        await taskRepository.UpdateRangeAsync(dailyTasks);
        await unitOfWork.CommitAsync();
    }

    private async Task UpdateUserTreeIfNeeded(Tasks task, CompleteTaskDto completeTaskDto)
    {
        if (task.UserTreeId == null)
        {
            if (!completeTaskDto.UserTreeId.HasValue)
                throw new ArgumentNullException(nameof(completeTaskDto),
                    "UserTreeId is required when task doesn't have an assigned tree.");

            var userTree = await userTreeRepository.GetByIdAsync(completeTaskDto.UserTreeId.Value)
                           ?? throw new KeyNotFoundException(
                               $"UserTree with ID {completeTaskDto.UserTreeId.Value} not found.");

            task.UserTreeId = userTree.UserTreeId;
            task.UserTree = userTree;
        }
        else
        {
            if (completeTaskDto.UserTreeId.HasValue && task.UserTreeId != completeTaskDto.UserTreeId.Value)
            {
                var userTree = await userTreeRepository.GetByIdAsync(completeTaskDto.UserTreeId.Value)
                               ?? throw new KeyNotFoundException(
                                   $"UserTree with ID {completeTaskDto.UserTreeId.Value} not found.");

                task.UserTreeId = userTree.UserTreeId;
                task.UserTree = userTree;
            }
        }
    }

    private async Task UpdateChallengeProgress(Tasks task)
    {
        var cloneFromTaskId = task.CloneFromTaskId;
        if (cloneFromTaskId == null || task.UserTree?.UserId == null)
            return;

        var challengeTask = await challengeTaskRepository.GetByTaskIdAsync(cloneFromTaskId.Value);
        if (challengeTask != null)
            await userChallengeService.UpdateUserChallengeProgressAsync(
                task.UserTree.UserId.Value,
                challengeTask.ChallengeId
            );
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

        // Tạo danh sách lỗi tổng hợp
        var errors = new List<ValidationFailure>(validationResult.Errors);

        // Kiểm tra tồn tại của TaskType
        var taskType = await taskTypeRepository.GetByIdAsync(dto.TaskTypeId);
        if (taskType == null)
            errors.Add(new ValidationFailure("TaskTypeId", $"TaskType not found. TaskTypeId = {dto.TaskTypeId}"));

        // Kiểm tra tồn tại của UserTree nếu có
        if (dto.UserTreeId is > 0)
        {
            var userTree = await userTreeRepository.GetByIdAsync(dto.UserTreeId.Value);
            if (userTree == null)
                errors.Add(new ValidationFailure("UserTreeId", $"UserTree not found. UserTreeId = {dto.UserTreeId}"));
        }

        // Kiểm tra cài đặt FocusMethod
        if (dto.FocusMethodId.HasValue && (dto.WorkDuration.HasValue || dto.BreakTime.HasValue))
        {
            var focusMethodErrors = await ValidateFocusMethodSettings(dto);
            errors.AddRange(focusMethodErrors);
        }

        // Nếu có bất kỳ lỗi nào, ném ValidationException
        if (errors.Count != 0) throw new ValidationException(errors);
    }

    private async Task<List<ValidationFailure>> ValidateFocusMethodSettings(CreateTaskDto dto)
    {
        var errors = new List<ValidationFailure>();

        if (dto.FocusMethodId == null) return errors;

        var method = await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value);
        if (method == null)
        {
            errors.Add(new ValidationFailure("FocusMethodId", $"FocusMethod not found. Id = {dto.FocusMethodId}"));
            return errors;
        }

        // Kiểm tra WorkDuration
        if (dto.WorkDuration.HasValue && method is { MinDuration: not null, MaxDuration: not null })
        {
            if (dto.WorkDuration.Value < method.MinDuration.Value)
                errors.Add(new ValidationFailure("WorkDuration",
                    $"Work duration must be at least {method.MinDuration.Value} minutes for the selected focus method."));
            else if (dto.WorkDuration.Value > method.MaxDuration.Value)
                errors.Add(new ValidationFailure("WorkDuration",
                    $"Work duration cannot exceed {method.MaxDuration.Value} minutes for the selected focus method."));
        }

        // Kiểm tra BreakTime
        if (!dto.BreakTime.HasValue || method is not { MinBreak: not null, MaxBreak: not null }) return errors;
        if (dto.BreakTime.Value < method.MinBreak.Value)
            errors.Add(new ValidationFailure("BreakTime",
                $"Break time must be at least {method.MinBreak.Value} minutes for the selected focus method."));
        else if (dto.BreakTime.Value > method.MaxBreak.Value)
            errors.Add(new ValidationFailure("BreakTime",
                $"Break time cannot exceed {method.MaxBreak.Value} minutes for the selected focus method."));

        return errors;
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

    private async Task<int?> GetDailyTaskTypeIdAsync()
    {
        try
        {
            return await taskTypeRepository.GetTaskTypeIdByNameAsync("daily");
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    private async Task<bool> IsDailyTaskAlreadyCompleted(Tasks task)
    {
        var dailyTaskTypeId = await GetDailyTaskTypeIdAsync();

        if (!dailyTaskTypeId.HasValue || task.TaskTypeId != dailyTaskTypeId.Value)
            return false;
        return task.CompletedAt.HasValue && task.CompletedAt.Value.Date == DateTime.UtcNow.Date;
    }

    // private async Task<bool> IsDailyTask(int taskTypeId)
    // {
    //     var dailyTaskTypeId = await GetDailyTaskTypeIdAsync();
    //     return taskTypeId == dailyTaskTypeId;
    // }

    private static int CalculateRemainingSeconds(Tasks task)
    {
        var totalDuration = TimeSpan.FromMinutes(task.TotalDuration ?? 0);
        var accumulated = TimeSpan.FromMinutes(task.AccumulatedTime ?? 0);

        TimeSpan remaining;

        switch (task.Status)
        {
            case TasksStatus.InProgress when task.StartedAt != null:
                var elapsed = DateTime.UtcNow - task.StartedAt.Value;
                remaining = totalDuration - (accumulated + elapsed);
                break;

            case TasksStatus.Paused:
                remaining = totalDuration - accumulated;
                break;

            case TasksStatus.NotStarted:
                remaining = totalDuration;
                break;

            default:
                remaining = TimeSpan.Zero;
                break;
        }

        return (int)Math.Max(remaining.TotalSeconds, 0);
    }
    
    private async Task<double> CalculateXpWithBoostAsync(Tasks task, double baseXp)
    {
        var userId = task.UserTree.UserId ?? throw new InvalidOperationException("UserId is null.");

        var equippedItem = await bagRepository.GetEquippedItemAsync(userId, ItemType.xp_boostTree);
        if (equippedItem != null &&
            double.TryParse(equippedItem.Item.ItemDetail.Effect, out var effectPercent) &&
            effectPercent > 0)
        {
            var bonusXp = (int)Math.Floor(baseXp * (effectPercent / 100));
            await useItemService.UseItemXpBoostTree(userId);
            return baseXp + bonusXp;
        }

        return baseXp;
    }
}