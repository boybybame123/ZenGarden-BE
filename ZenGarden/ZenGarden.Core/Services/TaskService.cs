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
    INotificationService notificationService,
    IMapper mapper,
    IBagRepository bagRepository,
    IUseItemService useItemService,
    IRedisService redisService,
    IValidator<CreateTaskDto> createTaskValidator) : ITaskService
{
    private const string TaskCacheKeyPrefix = "task:";
    private const string UserTasksCacheKeyPrefix = "user:tasks:";
    private const string TreeTasksCacheKeyPrefix = "tree:tasks:";
    private const string AllTasksCacheKey = "all:tasks";
    private static readonly TimeSpan DefaultCacheExpiry = TimeSpan.FromMinutes(15);

    public async Task<List<TaskDto>> GetAllTaskAsync()
    {
        var cachedTasks = await redisService.GetAsync<List<TaskDto>>(AllTasksCacheKey);
        if (cachedTasks != null) return cachedTasks;

        var tasks = await taskRepository.GetAllWithDetailsAsync();
        var taskDto = mapper.Map<List<TaskDto>>(tasks);

        foreach (var (dto, entity) in taskDto.Zip(tasks))
        {
            var accumulatedSeconds = (int)((entity.AccumulatedTime ?? 0) * 60);
            var remainingSeconds = CalculateRemainingSeconds(entity);

            dto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);
            dto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
        }

        await redisService.SetAsync(AllTasksCacheKey, taskDto, DefaultCacheExpiry);

        return taskDto;
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int taskId)
    {
        // Try to get from cache first
        var cacheKey = $"{TaskCacheKeyPrefix}{taskId}";
        var cachedTask = await redisService.GetAsync<TaskDto>(cacheKey);
        if (cachedTask != null) return cachedTask;

        var task = await taskRepository.GetTaskWithDetailsAsync(taskId);
        if (task == null) throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var taskDto = mapper.Map<TaskDto>(task);
        var remaining = CalculateRemainingSeconds(task);
        taskDto.RemainingTime = StringHelper.FormatSecondsToTime(remaining);
        var accumulatedSeconds = (int)((task.AccumulatedTime ?? 0) * 60);
        taskDto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);

        // Cache the result
        await redisService.SetAsync(cacheKey, taskDto, DefaultCacheExpiry);

        return taskDto;
    }

    public async Task<List<TaskDto>> GetTaskByUserIdAsync(int userId)
    {
        // Try to get from cache first
        var cacheKey = $"{UserTasksCacheKeyPrefix}{userId}";
        var cachedTasks = await redisService.GetAsync<List<TaskDto>>(cacheKey);
        if (cachedTasks != null) return cachedTasks;

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

        // Cache the result
        await redisService.SetAsync(cacheKey, taskDto, DefaultCacheExpiry);

        return taskDto;
    }

    public async Task<List<TaskDto>> GetTaskByUserTreeIdAsync(int userTreeId)
    {
        // Try to get from cache first
        var cacheKey = $"{TreeTasksCacheKeyPrefix}{userTreeId}";
        var cachedTasks = await redisService.GetAsync<List<TaskDto>>(cacheKey);
        if (cachedTasks != null) return cachedTasks;

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

        // Cache the result
        await redisService.SetAsync(cacheKey, taskDto, DefaultCacheExpiry);

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
        int? nextPriority = null;
        if (dto.TaskTypeId is 2 or 3 && dto.UserTreeId != null)
            nextPriority = await taskRepository.GetNextPriorityForTreeAsync(dto.UserTreeId.Value);

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
            Priority = nextPriority ?? 0,
            CreatedAt = DateTime.UtcNow,
            Status = TasksStatus.NotStarted,
            IsSuggested = dto.WorkDuration == null && dto.BreakTime == null
        };

        await taskRepository.CreateAsync(newTask);
        await unitOfWork.CommitAsync();
        await InvalidateTaskCaches(newTask);
        return mapper.Map<TaskDto>(newTask);
    }

    public async Task UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto)
    {
        var existingTask = await taskRepository.GetByIdAsync(taskId)
                           ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        var userid = await taskRepository.GetUserIdByTaskIdAsync(taskId) ??
                     throw new InvalidOperationException("UserId is null.");

        if (existingTask.Status is TasksStatus.InProgress or TasksStatus.Paused)
            throw new InvalidOperationException(
                "Tasks in 'InProgress' or 'Paused' state cannot be updated based on the new requirement.");

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskName))
            existingTask.TaskName = updateTaskDto.TaskName;

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskDescription))
            existingTask.TaskDescription = updateTaskDto.TaskDescription;

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskNote))
            existingTask.TaskNote = updateTaskDto.TaskNote;

        existingTask.TaskResult =
            await HandleTaskResultUpdate(updateTaskDto.TaskFile, updateTaskDto.TaskResult, userid);

        if (updateTaskDto.TotalDuration.HasValue)
            existingTask.TotalDuration = updateTaskDto.TotalDuration.Value;

        if (updateTaskDto.FocusMethodId.HasValue)
        {
            _ = await focusMethodRepository.GetByIdAsync(updateTaskDto.FocusMethodId.Value)
                ?? throw new KeyNotFoundException(
                    $"FocusMethod with ID {updateTaskDto.FocusMethodId.Value} not found.");

            existingTask.FocusMethodId = updateTaskDto.FocusMethodId.Value;
        }

        if (updateTaskDto.WorkDuration.HasValue)
            existingTask.WorkDuration = updateTaskDto.WorkDuration.Value;

        if (updateTaskDto.BreakTime.HasValue)
            existingTask.BreakTime = updateTaskDto.BreakTime.Value;

        if (updateTaskDto.StartDate.HasValue)
            existingTask.StartDate = updateTaskDto.StartDate.Value;

        if (updateTaskDto.EndDate.HasValue)
            existingTask.EndDate = updateTaskDto.EndDate.Value;

        if (updateTaskDto.UserTreeId.HasValue)
        {
            var newUserTree = await userTreeRepository.GetByIdAsync(updateTaskDto.UserTreeId.Value)
                              ?? throw new KeyNotFoundException(
                                  $"UserTree with ID {updateTaskDto.UserTreeId.Value} not found.");

            if (existingTask.UserTree != null && existingTask.UserTree.UserId != newUserTree.UserId)
                throw new InvalidOperationException("Cannot assign a UserTree that belongs to a different user.");

            existingTask.UserTreeId = updateTaskDto.UserTreeId.Value;
        }

        existingTask.UpdatedAt = DateTime.UtcNow;
        taskRepository.Update(existingTask);

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update task.");
        await InvalidateTaskCaches(existingTask);
    }


    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        await InvalidateTaskCaches(task);
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
        await InvalidateTaskCaches(task);
    }

    public async Task<double> CompleteTaskAsync(int taskId, CompleteTaskDto completeTaskDto)
    {
        double xpEarned = 0;

        var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");


        var userid = await taskRepository.GetUserIdByTaskIdAsync(taskId) ??
                     throw new InvalidOperationException("UserId is null.");

        if (task.TaskTypeId == 4)
        {
            if (string.IsNullOrWhiteSpace(completeTaskDto.TaskNote))
                throw new InvalidOperationException("TaskNote is required for challenge tasks.");

            if (string.IsNullOrWhiteSpace(completeTaskDto.TaskResult))
                throw new InvalidOperationException("TaskResult is required for challenge tasks.");

            task.TaskNote = completeTaskDto.TaskNote;
            task.TaskResult =
                await HandleTaskResultUpdate(completeTaskDto.TaskFile, completeTaskDto.TaskResult, userid);
        }

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

                if (task.TaskTypeId is 2 or 3 && task.Priority is { } priority)
                {
                    var decayRate = xpConfig.PriorityDecayRate ?? 0.1;
                    var minMultiplier = xpConfig.MinDecayMultiplier ?? 0.3;

                    var rawMultiplier = 1 - (priority - 1) * decayRate;
                    var decayMultiplier = Math.Max(rawMultiplier, minMultiplier);

                    baseXp *= decayMultiplier;
                }


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


                await notificationService.PushNotificationAsync(userid, "Task Completed",
                    $"Task {task.TaskName} has been completed. You earned {xpEarned} XP.");
                await InvalidateTaskCaches(task);
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

            await notificationService.PushNotificationAsync(userid, "Task Completed",
                $"Task {task.TaskName} has been completed. You earned {xpEarned} XP.");

            await InvalidateTaskCaches(task);
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
            if (task.UserTree?.UserId != null)
                await redisService.RemoveAsync($"{UserTasksCacheKeyPrefix}{task.UserTree.UserId}");

            if (task.UserTreeId != null)
                await redisService.RemoveAsync($"{TreeTasksCacheKeyPrefix}{task.UserTreeId}");
            await redisService.RemoveAsync($"{TaskCacheKeyPrefix}{task.TaskId}");
        }

        if (overdueTasks.Count != 0)
            await unitOfWork.CommitAsync();

        await redisService.RemoveAsync(AllTasksCacheKey);
        await redisService.RemoveByPatternAsync($"{UserTasksCacheKeyPrefix}*");
        await redisService.RemoveByPatternAsync($"{TreeTasksCacheKeyPrefix}*");
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
        await InvalidateTaskCaches(task);
    }


    public async Task AutoPauseTasksAsync()
    {
        var thresholdTime = DateTime.UtcNow.AddMinutes(-10);
        if (thresholdTime == default) throw new ArgumentException("Threshold time cannot be default or null.");

        var inProgressTasks = await taskRepository.GetTasksInProgressBeforeAsync(thresholdTime);
        var affectedUserIds = new HashSet<int>();
        var affectedTreeIds = new HashSet<int>();

        foreach (var task in inProgressTasks)
        {
            task.Status = TasksStatus.Paused;
            task.PausedAt = DateTime.UtcNow;
            taskRepository.Update(task);

            // Collect affected user and tree IDs
            if (task.UserTree?.UserId != null)
                affectedUserIds.Add(task.UserTree.UserId.Value);

            if (task.UserTreeId != null)
                affectedTreeIds.Add(task.UserTreeId.Value);

            // Invalidate individual task cache
            await redisService.RemoveAsync($"{TaskCacheKeyPrefix}{task.TaskId}");
        }

        if (inProgressTasks.Count > 0)
            await unitOfWork.CommitAsync();

        // Invalidate all task cache
        await redisService.RemoveAsync(AllTasksCacheKey);

        // Invalidate specific user and tree caches
        foreach (var userId in affectedUserIds)
            await redisService.RemoveAsync($"{UserTasksCacheKeyPrefix}{userId}");

        foreach (var treeId in affectedTreeIds)
            await redisService.RemoveAsync($"{TreeTasksCacheKeyPrefix}{treeId}");

        // To be extra safe, also invalidate all user and tree caches
        await redisService.RemoveByPatternAsync($"{UserTasksCacheKeyPrefix}*");
        await redisService.RemoveByPatternAsync($"{TreeTasksCacheKeyPrefix}*");
    }

    public async Task ResetDailyTasksAsync()
    {
        var dailyTasks = await taskRepository.GetDailyTasksAsync();
        var affectedUserIds = new HashSet<int>();
        var affectedTreeIds = new HashSet<int>();

        foreach (var task in dailyTasks)
        {
            task.CompletedAt = null;
            task.Status = TasksStatus.NotStarted;

            // Collect affected user and tree IDs
            if (task.UserTree?.UserId != null)
                affectedUserIds.Add(task.UserTree.UserId.Value);

            if (task.UserTreeId != null)
                affectedTreeIds.Add(task.UserTreeId.Value);

            // Invalidate individual task cache
            await redisService.RemoveAsync($"{TaskCacheKeyPrefix}{task.TaskId}");
        }

        await taskRepository.UpdateRangeAsync(dailyTasks);
        await unitOfWork.CommitAsync();

        // Invalidate all task cache
        await redisService.RemoveAsync(AllTasksCacheKey);

        // Invalidate specific user and tree caches
        foreach (var userId in affectedUserIds)
            await redisService.RemoveAsync($"{UserTasksCacheKeyPrefix}{userId}");

        foreach (var treeId in affectedTreeIds)
            await redisService.RemoveAsync($"{TreeTasksCacheKeyPrefix}{treeId}");

        // To be extra safe, also invalidate all user and tree caches
        await redisService.RemoveByPatternAsync($"{UserTasksCacheKeyPrefix}*");
        await redisService.RemoveByPatternAsync($"{TreeTasksCacheKeyPrefix}*");
    }

    public async Task ReorderTasksAsync(int userTreeId, List<ReorderTaskDto> reorderList)
    {
        if (reorderList.Count == 0)
            throw new ArgumentException("Reorder list cannot be empty.");

        var taskIds = reorderList.Select(x => x.TaskId).ToList();

        var tasks = await taskRepository.GetTasksByIdsAsync(taskIds);
        tasks = tasks.Where(t => t.TaskTypeId is 2 or 3).ToList();

        if (tasks.Count != reorderList.Count)
            throw new KeyNotFoundException("Some tasks not found.");

        var firstTaskTypeId = tasks.First().TaskTypeId;
        foreach (var task in tasks)
        {
            if (task.UserTreeId != userTreeId)
                throw new InvalidOperationException($"Task {task.TaskId} does not belong to the specified tree.");

            if (task.TaskTypeId != firstTaskTypeId)
                throw new InvalidOperationException(
                    $"Task {task.TaskId} has a different TaskTypeId ({task.TaskTypeId}) than expected ({firstTaskTypeId}). " +
                    "Tasks with different TaskTypeIds cannot be reordered together.");
        }

        foreach (var task in tasks)
        {
            var newPriority = reorderList.First(x => x.TaskId == task.TaskId).Priority;
            task.Priority = newPriority;
        }

        await taskRepository.UpdateRangeAsync(tasks);
        await unitOfWork.CommitAsync();
        await redisService.RemoveAsync($"{TreeTasksCacheKeyPrefix}{userTreeId}");
        await redisService.RemoveAsync(AllTasksCacheKey);

        foreach (var taskId in taskIds) await redisService.RemoveAsync($"{TaskCacheKeyPrefix}{taskId}");
    }

    public async Task WeeklyTaskPriorityResetAsync()
    {
        var allUserTrees = await userTreeRepository.GetAllActiveUserTreesAsync();
        var affectedUserIds = new HashSet<int>();

        foreach (var userTree in allUserTrees)
        {
            if (userTree.UserId.HasValue)
                affectedUserIds.Add(userTree.UserId.Value);

            var activeTasks = await taskRepository.GetActiveTasksByUserTreeIdAsync(userTree.UserTreeId);

            // Sort tasks by current priority
            var orderedTasks = activeTasks
                .OrderBy(t => t.Priority)
                .ToList();

            // Reassign priorities from 1 to n
            for (var i = 0; i < orderedTasks.Count; i++)
                orderedTasks[i].Priority = i + 1;

            if (orderedTasks.Count != 0)
                await taskRepository.UpdateRangeAsync(orderedTasks);

            // Invalidate tree-specific cache
            await redisService.RemoveAsync($"{TreeTasksCacheKeyPrefix}{userTree.UserTreeId}");

            // Invalidate individual task caches
            foreach (var task in orderedTasks) await redisService.RemoveAsync($"{TaskCacheKeyPrefix}{task.TaskId}");
        }

        await unitOfWork.CommitAsync();

        // Invalidate all task cache
        await redisService.RemoveAsync(AllTasksCacheKey);

        // Invalidate user-specific caches
        foreach (var userId in affectedUserIds)
            await redisService.RemoveAsync($"{UserTasksCacheKeyPrefix}{userId}");

        // To be extra safe, also invalidate all user caches
        await redisService.RemoveByPatternAsync($"{UserTasksCacheKeyPrefix}*");
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

    private async Task<string> HandleTaskResultUpdate(IFormFile? taskResultFile, string? taskResultUrl, int userid)
    {
        if (taskResultFile != null)
            return await s3Service.UploadFileToTaskUserFolderAsync(taskResultFile, userid);

        if (string.IsNullOrWhiteSpace(taskResultUrl)) return string.Empty;
        if (Uri.IsWellFormedUriString(taskResultUrl, UriKind.Absolute))
            return taskResultUrl;
        throw new ArgumentException("Invalid TaskResult URL.");
    }


    private async Task ValidateTaskDto(CreateTaskDto dto)
    {
        const int maxTaskPerUser = 20;
        var errors = new List<ValidationFailure>();

        var validationResult = await createTaskValidator.ValidateAsync(dto);
        errors.AddRange(validationResult.Errors);

        var taskType = await taskTypeRepository.GetByIdAsync(dto.TaskTypeId);
        if (taskType == null)
            errors.Add(new ValidationFailure("TaskTypeId", $"TaskType not found. TaskTypeId = {dto.TaskTypeId}"));

        switch (dto.UserTreeId)
        {
            case > 0 when await userTreeRepository.GetByIdAsync(dto.UserTreeId.Value) is { } userTree:
            {
                var userId = userTree.UserId;
                if (userId == null)
                {
                    errors.Add(new ValidationFailure("UserTreeId", "UserId is null in UserTree."));
                }
                else
                {
                    var userTasks = await taskRepository.GetTasksByUserIdAsync(userId.Value);
                    var activeTasksCount = userTasks.Count(t =>
                        t.Status is TasksStatus.NotStarted or TasksStatus.InProgress or TasksStatus.Paused);

                    if (activeTasksCount >= maxTaskPerUser)
                        errors.Add(new ValidationFailure("UserTreeId",
                            $"You can only have up to {maxTaskPerUser} active tasks."));
                }

                break;
            }
            case > 0:
                errors.Add(new ValidationFailure("UserTreeId", $"UserTree not found. UserTreeId = {dto.UserTreeId}"));
                break;
        }

        // 4. Validate FocusMethod nếu có
        if (dto.FocusMethodId.HasValue && (dto.WorkDuration.HasValue || dto.BreakTime.HasValue))
        {
            var focusMethodErrors = await ValidateFocusMethodSettings(dto);
            errors.AddRange(focusMethodErrors);
        }

        // 5. Throw nếu có lỗi
        if (errors.Count > 0)
            throw new ValidationException(errors);
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

        if (dto.WorkDuration.HasValue && method is { MinDuration: not null, MaxDuration: not null })
        {
            if (dto.WorkDuration.Value < method.MinDuration.Value)
                errors.Add(new ValidationFailure("WorkDuration",
                    $"Work duration must be at least {method.MinDuration.Value} minutes for the selected focus method."));
            else if (dto.WorkDuration.Value > method.MaxDuration.Value)
                errors.Add(new ValidationFailure("WorkDuration",
                    $"Work duration cannot exceed {method.MaxDuration.Value} minutes for the selected focus method."));
        }

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

            case TasksStatus.Completed:
            case TasksStatus.Overdue:
            case TasksStatus.Canceled:
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
        if (equippedItem == null ||
            !double.TryParse(equippedItem.Item.ItemDetail.Effect, out var effectPercent) ||
            !(effectPercent > 0)) return baseXp;
        var bonusXp = (int)Math.Floor(baseXp * (effectPercent / 100));
        await useItemService.UseItemXpBoostTree(userId);
        return baseXp + bonusXp;
    }

    private async Task InvalidateTaskCaches(Tasks task)
    {
        // Invalidate individual task cache
        await redisService.RemoveAsync($"{TaskCacheKeyPrefix}{task.TaskId}");

        // Invalidate all tasks cache
        await redisService.RemoveAsync(AllTasksCacheKey);

        // Invalidate user-specific cache if applicable
        if (task.UserTree?.UserId != null)
            await redisService.RemoveAsync($"{UserTasksCacheKeyPrefix}{task.UserTree.UserId}");

        // Invalidate tree-specific cache if applicable
        if (task.UserTreeId != null)
            await redisService.RemoveAsync($"{TreeTasksCacheKeyPrefix}{task.UserTreeId}");
    }
}