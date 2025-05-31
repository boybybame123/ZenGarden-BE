using AutoMapper;
using FluentValidation;
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
    IRedisService redisService,
    IUserXpLogService userXpLogService,
    IValidator<CreateTaskDto> createTaskValidator,
    TaskRealtimeService taskRealtimeService,
    IUseItemService useItemService,
    IItemRepository itemRepository
    ) : ITaskService
{
    private const string TaskCacheKeyPrefix = "task:";
    private const string UserTasksCacheKeyPrefix = "user:tasks:";
    private const string TreeTasksCacheKeyPrefix = "tree:tasks:";
    private const string AllTasksCacheKey = "all:tasks";
    private const string UserChallengeCacheKeyPrefix = "user_challenge_progress_";
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
        var cacheKey = $"{TaskCacheKeyPrefix}{taskId}";
        var cachedTask = await redisService.GetAsync<TaskDto>(cacheKey);
        if (cachedTask != null) return cachedTask;

        var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var taskDto = mapper.Map<TaskDto>(task);
        var remaining = CalculateRemainingSeconds(task);
        taskDto.RemainingTime = StringHelper.FormatSecondsToTime(remaining);
        var accumulatedSeconds = (int)((task.AccumulatedTime ?? 0) * 60);
        taskDto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);

        await redisService.SetAsync(cacheKey, taskDto, DefaultCacheExpiry);
        return taskDto;
    }

    public async Task<List<TaskDto>> GetTaskByUserIdAsync(int userId)
    {
        //var cacheKey = $"{UserTasksCacheKeyPrefix}{userId}";
        //var cachedTasks = await redisService.GetAsync<List<TaskDto>>(cacheKey);
        //if (cachedTasks != null) return cachedTasks;

        var tasks = await taskRepository.GetTasksByUserIdAsync(userId)
                    ?? throw new KeyNotFoundException($"Tasks with User ID {userId} not found.");

        var taskDto = mapper.Map<List<TaskDto>>(tasks);
        foreach (var (dto, entity) in taskDto.Zip(tasks))
        {
            var accumulatedSeconds = (int)((entity.AccumulatedTime ?? 0) * 60);
            var remainingSeconds = CalculateRemainingSeconds(entity);

            dto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);
            dto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
        }

        //await redisService.SetAsync(cacheKey, taskDto, DefaultCacheExpiry);
        return taskDto;
    }

    public async Task<List<TaskDto>> GetTaskByUserTreeIdAsync(int userTreeId)
    {
        //var cacheKey = $"{TreeTasksCacheKeyPrefix}{userTreeId}";
        //var cachedTasks = await redisService.GetAsync<List<TaskDto>>(cacheKey);
        //if (cachedTasks != null) return cachedTasks;

        var tasks = await taskRepository.GetTasksByUserTreeIdAsync(userTreeId)
                    ?? throw new KeyNotFoundException($"Tasks with UserTree ID {userTreeId} not found.");

        var taskDto = mapper.Map<List<TaskDto>>(tasks);
        foreach (var (dto, entity) in taskDto.Zip(tasks))
        {
            var accumulatedSeconds = (int)((entity.AccumulatedTime ?? 0) * 60);
            var remainingSeconds = CalculateRemainingSeconds(entity);

            dto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);
            dto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
        }

        //await redisService.SetAsync(cacheKey, taskDto, DefaultCacheExpiry);
        return taskDto;
    }

    public async Task<TaskDto> CreateTaskWithSuggestedMethodAsync(CreateTaskDto dto)
     {
         // Validate using FluentValidation
         var validationResult = await createTaskValidator.ValidateAsync(dto);
         if (!validationResult.IsValid)
             throw new ValidationException(validationResult.Errors);

         // Validate task type exists
         if (await taskTypeRepository.GetByIdAsync(dto.TaskTypeId) == null)
             throw new KeyNotFoundException($"TaskType with ID {dto.TaskTypeId} not found.");

         // Skip UserTreeId and UserId validation for challenge tasks (TaskTypeId = 4)
         if (dto.UserTreeId.HasValue && dto.TaskTypeId != 4)
         {
             var userTree = await userTreeRepository.GetByIdAsync(dto.UserTreeId.Value)
                            ?? throw new KeyNotFoundException($"UserTree with ID {dto.UserTreeId.Value} not found.");

             if (userTree.UserId == null)
                 throw new InvalidOperationException("UserId is null in UserTree.");
         }

         // Validate focus method settings if provided
         if (dto.FocusMethodId.HasValue && (dto.WorkDuration.HasValue || dto.BreakTime.HasValue))
         {
             var method = await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value)
                          ?? throw new KeyNotFoundException($"FocusMethod with ID {dto.FocusMethodId} not found.");

             if (dto.WorkDuration.HasValue && method is { MinDuration: not null, MaxDuration: not null })
             {
                 if (dto.WorkDuration.Value < method.MinDuration.Value)
                     throw new InvalidOperationException(
                         $"Work duration must be at least {method.MinDuration.Value} minutes for the selected focus method.");
                 if (dto.WorkDuration.Value > method.MaxDuration.Value)
                     throw new InvalidOperationException(
                         $"Work duration cannot exceed {method.MaxDuration.Value} minutes for the selected focus method.");
             }

             if (dto.BreakTime.HasValue && method is { MinBreak: not null, MaxBreak: not null })
             {
                 if (dto.BreakTime.Value < method.MinBreak.Value)
                     throw new InvalidOperationException(
                         $"Break time must be at least {method.MinBreak.Value} minutes for the selected focus method.");
                 if (dto.BreakTime.Value > method.MaxBreak.Value)
                     throw new InvalidOperationException(
                         $"Break time cannot exceed {method.MaxBreak.Value} minutes for the selected focus method.");
             }
         }

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

         // Load task with TaskType for mapping
         var taskWithDetails = await taskRepository.GetTaskWithDetailsAsync(newTask.TaskId);
         var taskDto = mapper.Map<TaskDto>(taskWithDetails);
         var remainingSeconds = CalculateRemainingSeconds(newTask);
         taskDto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
         var accumulatedSeconds = (int)((newTask.AccumulatedTime ?? 0) * 60);
         taskDto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);

         await taskRealtimeService.NotifyTaskCreated(taskDto);
         return taskDto;
     }

    public async Task<List<TaskDto>> CreateMultipleTasksWithSuggestedMethodAsync(List<CreateTaskDto> dtos)
    {
        var createdTasks = new List<TaskDto>();
        var tasksToCreate = new List<Tasks>();
        var nextPriorities = new Dictionary<int, int>();

        // Validate all tasks first
        foreach (var dto in dtos)
        {
            var validationResult = await createTaskValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            if (await taskTypeRepository.GetByIdAsync(dto.TaskTypeId) == null)
                throw new KeyNotFoundException($"TaskType with ID {dto.TaskTypeId} not found.");

            // Skip UserTreeId and UserId validation for challenge tasks (TaskTypeId = 4)
            if (dto.UserTreeId.HasValue && dto.TaskTypeId != 4)
            {
                var userTree = await userTreeRepository.GetByIdAsync(dto.UserTreeId.Value)
                               ?? throw new KeyNotFoundException($"UserTree with ID {dto.UserTreeId.Value} not found.");

                if (userTree.UserId == null)
                    throw new InvalidOperationException("UserId is null in UserTree.");

                // Get next priority for this tree if needed
                if (dto.TaskTypeId is 2 or 3 && !nextPriorities.ContainsKey(dto.UserTreeId.Value))
                {
                    nextPriorities[dto.UserTreeId.Value] = await taskRepository.GetNextPriorityForTreeAsync(dto.UserTreeId.Value);
                }
            }

            if (!dto.FocusMethodId.HasValue || (!dto.WorkDuration.HasValue && !dto.BreakTime.HasValue)) continue;
            var method = await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value)
                         ?? throw new KeyNotFoundException($"FocusMethod with ID {dto.FocusMethodId} not found.");

            if (dto.BreakTime.HasValue && method is { MinBreak: not null, MaxBreak: not null })
            {
                if (dto.BreakTime.Value < method.MinBreak.Value)
                    throw new InvalidOperationException(
                        $"Break time must be at least {method.MinBreak.Value} minutes for the selected focus method.");
                if (dto.BreakTime.Value > method.MaxBreak.Value)
                    throw new InvalidOperationException(
                        $"Break time cannot exceed {method.MaxBreak.Value} minutes for the selected focus method.");
            }
        }

        // Create all tasks
        foreach (var dto in dtos)
        {
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
                Priority = dto is { UserTreeId: not null, TaskTypeId: 2 or 3 } ? nextPriorities[dto.UserTreeId.Value]++ : 0,
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted,
                IsSuggested = dto.WorkDuration == null && dto.BreakTime == null
            };

            tasksToCreate.Add(newTask);
        }

        // Save all tasks at once
        await taskRepository.AddRangeAsync(tasksToCreate);
        await unitOfWork.CommitAsync();

        // Create DTOs and notify
        foreach (var task in tasksToCreate)
        {
            await InvalidateTaskCaches(task);
            var taskDto = mapper.Map<TaskDto>(task);
            var remainingSeconds = CalculateRemainingSeconds(task);
            taskDto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
            var accumulatedSeconds = (int)((task.AccumulatedTime ?? 0) * 60);
            taskDto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);

            await taskRealtimeService.NotifyTaskCreated(taskDto);
            createdTasks.Add(taskDto);
        }

        return createdTasks;
    }

    public async Task UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto)
    {
        var existingTask = await taskRepository.GetByIdAsync(taskId)
                           ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var userId = await taskRepository.GetUserIdByTaskIdAsync(taskId)
                     ?? throw new InvalidOperationException("UserId is null.");

        if (existingTask.Status is TasksStatus.InProgress or TasksStatus.Paused)
            throw new InvalidOperationException(
                "Tasks in 'InProgress' or 'Paused' state cannot be updated based on the new requirement.");

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskName))
            existingTask.TaskName = updateTaskDto.TaskName;

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskDescription))
            existingTask.TaskDescription = updateTaskDto.TaskDescription;

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskNote))
            existingTask.TaskNote = updateTaskDto.TaskNote;

        if (updateTaskDto.WorkDuration.HasValue)
            existingTask.WorkDuration = updateTaskDto.WorkDuration.Value;

        if (updateTaskDto.BreakTime.HasValue)
            existingTask.BreakTime = updateTaskDto.BreakTime.Value;

        if (updateTaskDto.StartDate.HasValue)
            existingTask.StartDate = updateTaskDto.StartDate.Value;

        if (updateTaskDto.EndDate.HasValue)
            existingTask.EndDate = updateTaskDto.EndDate.Value;

        if (updateTaskDto.AccumulatedTime.HasValue)
            existingTask.AccumulatedTime = updateTaskDto.AccumulatedTime.Value;

        if (updateTaskDto.TotalDuration.HasValue)
            existingTask.TotalDuration = updateTaskDto.TotalDuration.Value;

        if (updateTaskDto.TaskTypeId.HasValue)
            existingTask.TaskTypeId = updateTaskDto.TaskTypeId.Value;

        if (updateTaskDto.FocusMethodId.HasValue)
            existingTask.FocusMethodId = updateTaskDto.FocusMethodId.Value;

        if (existingTask is { StartDate: not null, EndDate: not null })
            if (existingTask.StartDate > existingTask.EndDate)
                throw new InvalidOperationException(
                    $"StartDate cannot be after EndDate. StartDate: {existingTask.StartDate:u}, EndDate: {existingTask.EndDate:u}");

        if (updateTaskDto.UserTreeId.HasValue)
        {
            var newUserTree = await userTreeRepository.GetByIdAsync(updateTaskDto.UserTreeId.Value)
                              ?? throw new KeyNotFoundException(
                                  $"UserTree with ID {updateTaskDto.UserTreeId.Value} not found.");

            if (existingTask.UserTree != null && existingTask.UserTree.UserId != newUserTree.UserId)
                throw new InvalidOperationException("Cannot assign a UserTree that belongs to a different user.");

            existingTask.UserTreeId = updateTaskDto.UserTreeId.Value;
        }

        var newTaskResult = await HandleTaskResultUpdate(updateTaskDto.TaskFile, updateTaskDto.TaskResult, userId);
        if (newTaskResult != null)
        {
            existingTask.TaskResult = newTaskResult;
        }

        var needUpdateXpConfig = updateTaskDto.TotalDuration.HasValue
                                 || updateTaskDto.TaskTypeId.HasValue
                                 || updateTaskDto.FocusMethodId.HasValue;

        if (needUpdateXpConfig)
        {
            var taskTypeId = existingTask.TaskTypeId;
            var focusMethodId = existingTask.FocusMethodId;

            if (focusMethodId.HasValue)
                await xpConfigService.EnsureXpConfigExists(
                    focusMethodId.Value,
                    taskTypeId,
                    existingTask.TotalDuration ?? 30
                );
        }

        existingTask.UpdatedAt = DateTime.UtcNow;
        taskRepository.Update(existingTask);

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update task.");

        await InvalidateTaskCaches(existingTask);
        var taskDto = mapper.Map<TaskDto>(existingTask);
        await taskRealtimeService.NotifyTaskUpdated(taskDto);
    }

    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        
        var userId = task.UserTree?.UserId;
        var treeId = task.UserTreeId;

        // Delete associated challenge tasks first
        var challengeTasks = await challengeTaskRepository.GetAllByTaskIdAsync(taskId);
        
        if (challengeTasks.Count != 0)
        {
            foreach (var challengeTask in challengeTasks)
            {
                await challengeTaskRepository.RemoveAsync(challengeTask);
            }
            await unitOfWork.CommitAsync();
        }
        
        await InvalidateTaskCaches(task);
        await taskRepository.RemoveAsync(task);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete task.");
            
        await taskRealtimeService.NotifyTaskDeleted(taskId, userId, treeId);
    }

    public async Task StartTaskAsync(int taskId, int userId)
    {
        try
        {
            var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                       ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

            if (task.UserTree == null)
            {
                Console.WriteLine($"[ERROR] Task {taskId} has no UserTree associated");
                throw new InvalidOperationException("Task is not associated with any UserTree.");
            }

            var now = DateTime.UtcNow;

            Console.WriteLine($"[DEBUG] StartTask - Task ID: {taskId}");
            Console.WriteLine($"[DEBUG] StartTask - UserTree ID: {task.UserTreeId}");
            Console.WriteLine($"[DEBUG] StartTask - now: {now:u}, StartDate: {task.StartDate:u}, EndDate: {task.EndDate:u}, Status: {task.Status}");

            await ValidateTaskStartConditions(task, now, userId, taskId);
            await UpdateTaskStatus(task, now);

            Console.WriteLine("[DEBUG] Task status updated successfully");

            await InvalidateTaskCaches(task);
            var taskDto = mapper.Map<TaskDto>(task);
            await taskRealtimeService.NotifyTaskStatusChanged(taskDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== [StartTaskAsync ERROR] ===");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
            Console.WriteLine("==============================");

            throw;
        }
    }

    private async Task ValidateTaskStartConditions(Tasks task, DateTime now, int userId, int taskId)
    {
        if (now > task.EndDate)
            throw new InvalidOperationException(
                $"Task deadline has passed. Current time: {now:u}, EndDate: {task.EndDate:u}");

        if (task.Status == TasksStatus.InProgress)
            throw new InvalidOperationException("Task is already in progress.");

        var existingInProgressTask = await taskRepository.GetUserTaskInProgressAsync(userId);
        if (existingInProgressTask != null && existingInProgressTask.TaskId != taskId)
            throw new InvalidOperationException(
                $"You already have a task in progress (Task ID: {existingInProgressTask.TaskId}). Please complete it first.");
    }

    private async Task UpdateTaskStatus(Tasks task, DateTime now)
    {
        Console.WriteLine($"[DEBUG] UpdateTaskStatus - current task status: {task.Status}");
        Console.WriteLine($"[DEBUG] Task details - ID: {task.TaskId}, UserTreeId: {task.UserTreeId}");

        switch (task.Status)
        {
            case TasksStatus.NotStarted:
                task.StartedAt = now;
                task.Status = TasksStatus.InProgress;

                Console.WriteLine("[DEBUG] Task changed to InProgress");

                if (task.UserTree?.UserId != null)
                {
                    Console.WriteLine($"[DEBUG] Adding XP for User ID: {task.UserTree.UserId.Value}");
                    await userXpLogService.AddXpForStartTaskAsync(task.UserTree.UserId.Value);
                }
                else
                {
                    Console.WriteLine("[WARNING] task.UserTree or task.UserTree.UserId is null!");
                }
                break;

            case TasksStatus.Paused:
                if (task.PausedAt == null || task.StartedAt == null)
                    throw new InvalidOperationException("Invalid task timestamps for paused state.");

                if (task.AccumulatedTime >= task.TotalDuration)
                {
                    throw new InvalidOperationException($"Task has exceeded its duration limit of {task.TotalDuration} minutes.");
                }

                task.StartedAt = now;
                task.PausedAt = null;
                task.Status = TasksStatus.InProgress;
                break;

            case TasksStatus.InProgress:
            case TasksStatus.Completed:
            case TasksStatus.Overdue:
            case TasksStatus.Canceled:
            default:
                throw new InvalidOperationException($"Task cannot be started from the current state: {task.Status}");
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
        
        var userid = await taskRepository.GetUserIdByTaskIdAsync(taskId) ??
                     throw new InvalidOperationException("UserId is null.");

        if (!string.IsNullOrWhiteSpace(completeTaskDto.TaskNote))
        {
            task.TaskNote = completeTaskDto.TaskNote;
        }    
        task.TaskResult = await HandleTaskResultUpdate(completeTaskDto.TaskFile, completeTaskDto.TaskResult, userid);

        // Only require TaskResult for challenge tasks (tasks cloned from a challenge)
        if (task.CloneFromTaskId != null && string.IsNullOrWhiteSpace(task.TaskResult))
            throw new InvalidOperationException("TaskResult is required for challenge tasks.");
        
        await UpdateUserTreeIfNeeded(task, completeTaskDto);

        ValidateTaskForCompletion(task);

        if (task.FocusMethodId != null)
        {
            try
            {
                var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                               ?? throw new KeyNotFoundException("XP configuration not found for this task.");

                var baseXp = Math.Round(xpConfig.BaseXp * xpConfig.XpMultiplier, 2);

                baseXp = CalculateXpWithPriorityDecay(task, baseXp);
                baseXp = Math.Round(baseXp, 2);

                var bonusXp = 0.0;
                string? activeBoostItemName = null;

                // Check if user has equipped XP Boost Tree
                var hasEquippedXpBoostTree = await bagRepository.HasUsedXpBoostItemAsync(userid);
                if (hasEquippedXpBoostTree)
                {
                    var itemId = await useItemService.UseItemXpBoostTree(userid);
                    var activeItem = await itemRepository.GetByIdAsync(itemId);
                    if (activeItem?.ItemDetail?.Effect != null && 
                        double.TryParse(activeItem.ItemDetail.Effect, out var parsedEffect) && 
                        parsedEffect > 0)
                    {
                        double effectPercentage = parsedEffect;
                        activeBoostItemName = activeItem.Name;
                        bonusXp = Math.Round(baseXp * (effectPercentage / 100), 2);
                    }
                }
                
                xpEarned = Math.Round(baseXp + bonusXp, 2);

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
               

                await UpdateChallengeProgress(task);

                var xpMessage = bonusXp > 0 && activeBoostItemName != null
                    ? $"Task {task.TaskName} has been completed. You've earned {xpEarned} XP ({baseXp} XP + {activeBoostItemName}: +{bonusXp} XP) for completing a task!"
                    : $"Task {task.TaskName} has been completed. You've earned {xpEarned} XP for completing a task!";
                await notificationService.PushNotificationAsync(userid, "Task Completed", xpMessage);
                await InvalidateTaskCaches(task);
            }
            catch (Exception ex)
            {
                
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

        var taskDto = mapper.Map<TaskDto>(task);
        await taskRealtimeService.NotifyTaskStatusChanged(taskDto);
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
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (task.FocusMethodId == null)
            throw new InvalidOperationException("Task does not have an assigned FocusMethod.");

        var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                       ?? throw new KeyNotFoundException("XP configuration not found.");

        if (xpConfig.BaseXp <= 0)
            throw new InvalidOperationException("Invalid BaseXp value in XP configuration.");

        var baseXp = xpConfig.BaseXp * xpConfig.XpMultiplier;
        baseXp = CalculateXpWithPriorityDecay(task, baseXp);
        return Math.Round(baseXp, 2);
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
        var taskDto = mapper.Map<TaskDto>(task);
        await taskRealtimeService.NotifyTaskStatusChanged(taskDto);
    }

    public async Task UpdateTaskTypeAsync(int taskId, int newTaskTypeId, int newDuration)
    {
        var task = await taskRepository.GetByIdAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var validTypeChanges = new[] { 2, 3 };

        if (!validTypeChanges.Contains(task.TaskTypeId) || !validTypeChanges.Contains(newTaskTypeId))
            throw new InvalidOperationException("Only switching between TaskTypeId 2 and 3 is allowed.");

        if ((newTaskTypeId == 1 && newDuration is < 30 or > 180) ||
            (newTaskTypeId == 2 && newDuration < 180))
            throw new ArgumentException("Invalid duration for the selected task type.");


        var newTaskType = await taskTypeRepository.GetByIdAsync(newTaskTypeId);
        if (newTaskType == null)
            throw new KeyNotFoundException($"TaskType with ID {newTaskTypeId} not found.");

        task.TaskTypeId = newTaskTypeId;
        task.TotalDuration = newDuration;
        task.UpdatedAt = DateTime.UtcNow;
        taskRepository.Update(task);

        if (task is { FocusMethodId: not null, TotalDuration: not null })
            await xpConfigService.EnsureXpConfigExists(
                task.FocusMethodId.Value,
                newTaskTypeId,
                newDuration
            );

        if (task.UserTree?.UserId is { } userId)
            await notificationService.PushNotificationAsync(
                userId,
                "Task Type Updated",
                $"Your task '{task.TaskName}' has been changed to type {newTaskTypeId}."
            );

        await unitOfWork.CommitAsync();
        await InvalidateTaskCaches(task);
        var taskDto = mapper.Map<TaskDto>(task);
        await taskRealtimeService.NotifyTaskUpdated(taskDto);
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

    public async Task ReorderTasksAsync(int userTreeId, List<ReorderTaskDto> reorderList)
    {
        if (reorderList.Count == 0)
            throw new ArgumentException("Reorder list cannot be empty.");

        var taskIds = reorderList.Select(x => x.TaskId).ToList();

        var tasks = await taskRepository.GetReorderableTasksByIdsAsync(taskIds);

        if (tasks.Count != reorderList.Count)
            throw new KeyNotFoundException("Some tasks not found or cannot be reordered due to type or status.");

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
        foreach (var taskId in taskIds)
            await redisService.RemoveAsync($"{TaskCacheKeyPrefix}{taskId}");
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

    public async Task ForceUpdateTaskStatusAsync(int taskId, TasksStatus newStatus)
    {
        var task = await taskRepository.GetByIdAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        await ForceUpdateTaskStatusAsync(task, newStatus);
    }

    public async Task<List<Tasks>> GetTasksToNotifyAsync(DateTime currentTime)
    {
        var tasksToNotify = new List<Tasks>();

        // Trường hợp 1: Thông báo khi đến StartDate
        var startDateTasks = await taskRepository.GetTasksByStartDateTimeMatchingAsync(currentTime);
        tasksToNotify.AddRange(startDateTasks);

        // Trường hợp 2: Thông báo vào 7h sáng nếu đã qua startDate nhưng chưa start
        if (currentTime is { Hour: 7, Minute: 0 })
        {
            var passedStartDateTasks = await taskRepository.GetTasksWithPassedStartDateNotStartedAsync(currentTime);
            tasksToNotify.AddRange(passedStartDateTasks);
        }

        // Trường hợp 3: Thông báo trước EndDate 1 ngày vào 7h sáng
        if (currentTime is { Hour: 7, Minute: 0 })
        {
            var oneDayBeforeEndDate = currentTime.AddDays(1);
            var endDateReminderTasks = await taskRepository.GetTasksWithEndDateMatchingAsync(oneDayBeforeEndDate, true);
            tasksToNotify.AddRange(endDateReminderTasks);
        }

        // Trường hợp 4: Thông báo khi còn 5 phút trước EndDate
        var fiveMinutesLater = currentTime.AddMinutes(5);
        var urgentTasks = await taskRepository.GetTasksWithEndDateMatchingAsync(fiveMinutesLater, false);
        tasksToNotify.AddRange(urgentTasks);

        // Trường hợp 5: Thông báo khi task sắp hết TotalDuration
        var inProgressTasks = await taskRepository.GetTasksInProgressAsync();
        foreach (var task in inProgressTasks)
        {
            if (!task.TotalDuration.HasValue || !task.StartedAt.HasValue) continue;
            
            var accumulatedMinutes = task.AccumulatedTime ?? 0;
            var currentSessionMinutes = (currentTime - task.StartedAt.Value).TotalMinutes;
            var totalElapsedTime = accumulatedMinutes + currentSessionMinutes;
            var remainingTime = task.TotalDuration.Value - totalElapsedTime;
                
            if (remainingTime is <= 5 and > 0)
            {
                tasksToNotify.Add(task);
            }
        }

        return tasksToNotify;
    }

    public async Task UpdateTaskSimpleAsync(int taskId, UpdateTaskSimpleDto updateTaskDto)
    {
        var existingTask = await taskRepository.GetByIdAsync(taskId)
                           ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        existingTask.TotalDuration = updateTaskDto.TotalDuration;
        existingTask.UpdatedAt = DateTime.UtcNow;
        taskRepository.Update(existingTask);
        await unitOfWork.CommitAsync();
        await InvalidateTaskCaches(existingTask);
        var taskDto = mapper.Map<TaskDto>(existingTask);
        await taskRealtimeService.NotifyTaskUpdated(taskDto);
    }

    public async Task<List<TaskDto>> GetClonedTasksByUserChallengeAsync(int userId, int challengeId)
    {
        var cacheKey = $"{UserChallengeCacheKeyPrefix}{userId}:{challengeId}";

        var cachedTasks = await redisService.GetAsync<List<TaskDto>>(cacheKey);
        if (cachedTasks != null) return cachedTasks;

        var tasks = await taskRepository.GetClonedTasksByUserChallengeAsync(userId, challengeId);
        if (tasks == null || tasks.Count == 0)
            throw new KeyNotFoundException(
                $"No cloned tasks found for User ID {userId} and Challenge ID {challengeId}.");

        var taskDto = mapper.Map<List<TaskDto>>(tasks);
        foreach (var (dto, entity) in taskDto.Zip(tasks))
        {
            var accumulatedSeconds = (int)((entity.AccumulatedTime ?? 0) * 60);
            var remainingSeconds = CalculateRemainingSeconds(entity);

            dto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);
            dto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
        }

        await redisService.SetAsync(cacheKey, taskDto, DefaultCacheExpiry);

        return taskDto;
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

    private async Task<string?> HandleTaskResultUpdate(IFormFile? taskResultFile, string? taskResultUrl, int userid)
    {
        if (taskResultFile != null)
            return await s3Service.UploadFileToTaskUserFolderAsync(taskResultFile, userid);

        if (string.IsNullOrWhiteSpace(taskResultUrl)) return null;
        if (Uri.IsWellFormedUriString(taskResultUrl, UriKind.Absolute))
            return taskResultUrl;
        throw new ArgumentException("Invalid TaskResult URL.");
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
    }

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

    private async Task InvalidateTaskCaches(Tasks task)
    {
        var cacheKeys = new List<string>
        {
            $"{TaskCacheKeyPrefix}{task.TaskId}",
            AllTasksCacheKey
        };

        if (task.UserTreeId.HasValue)
            cacheKeys.Add($"{TreeTasksCacheKeyPrefix}{task.UserTreeId}");

        var userId = await taskRepository.GetUserIdByTaskIdAsync(task.TaskId);
        if (userId.HasValue)
            cacheKeys.Add($"{UserTasksCacheKeyPrefix}{userId}");

        await Task.WhenAll(cacheKeys.Select(redisService.RemoveAsync));
    }

    private async Task ForceUpdateTaskStatusAsync(Tasks task, TasksStatus newStatus)
    {
        var timestamp = DateTime.UtcNow;

        switch (newStatus)
        {
            case TasksStatus.InProgress:
                task.StartedAt = timestamp;
                task.PausedAt = null;
                break;
            case TasksStatus.Paused:
                task.StartedAt ??= timestamp.AddMinutes(-5);

                var delta = (timestamp - task.StartedAt.Value).TotalMinutes;
                task.AccumulatedTime = (task.AccumulatedTime ?? 0) + delta;
                task.PausedAt = timestamp;
                break;
            case TasksStatus.Completed:
                task.CompletedAt = timestamp;
                break;
            case TasksStatus.NotStarted:
                // Reset các giá trị
                task.StartedAt = null;
                task.PausedAt = null;
                task.CompletedAt = null;
                break;
            // Các trạng thái khác không cần logic đặc biệt
            case TasksStatus.Overdue:
            case TasksStatus.Canceled:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, null);
        }

        task.Status = newStatus;
        task.UpdatedAt = timestamp;

        taskRepository.Update(task);
        await unitOfWork.CommitAsync();
        await InvalidateTaskCaches(task);
    }

    private static double CalculateXpWithPriorityDecay(Tasks task, double baseXp)
    {
        if (task.TaskTypeId is not 2 and not 3 || task.Priority is null)
            return baseXp;

        const double decayFactor = 0.1; // k trong công thức e^(-k * priority)
        var minXp = baseXp * 0.3; // Không giảm dưới 30% base XP

        var priorityIndex = task.Priority.Value - 1;

        var decayMultiplier = Math.Exp(-decayFactor * priorityIndex);
        var xp = baseXp * decayMultiplier;

        return Math.Max(xp, minXp);
    }

    public async Task UpdateTaskResultAsync(int taskId, UpdateTaskResultDto updateTaskResultDto)
    {
        var existingTask = await taskRepository.GetByIdAsync(taskId)
                           ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var userId = await taskRepository.GetUserIdByTaskIdAsync(taskId)
                     ?? throw new InvalidOperationException("UserId is null.");

        if (!string.IsNullOrWhiteSpace(updateTaskResultDto.TaskNote))
            existingTask.TaskNote = updateTaskResultDto.TaskNote;

        var newTaskResult = await HandleTaskResultUpdate(
            updateTaskResultDto.TaskFile,
            updateTaskResultDto.TaskResult,
            userId
        );
        
        if (newTaskResult != null)
        {
            existingTask.TaskResult = newTaskResult;
        }

        existingTask.UpdatedAt = DateTime.UtcNow;
        taskRepository.Update(existingTask);
        await unitOfWork.CommitAsync();
        await InvalidateTaskCaches(existingTask);
        var taskDto = mapper.Map<TaskDto>(existingTask);
        await taskRealtimeService.NotifyTaskUpdated(taskDto);
    }

    public async Task<TaskXpInfoDto> GetTaskXpInfoAsync(int taskId)
    {
        var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var xpLog = await treeXpLogRepository.GetTreeXpLogByTaskIdAsync(taskId);
        if (xpLog.Count == 0)
        {
            // If no XP log exists, calculate potential XP
            var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId ?? 0)
                          ?? throw new KeyNotFoundException("XP configuration not found for this task.");

            var originalBaseXp = Math.Round(xpConfig.BaseXp * xpConfig.XpMultiplier, 2);
            var baseXp = CalculateXpWithPriorityDecay(task, originalBaseXp);

            // Calculate priority effect
            double? priorityMultiplier = null;
            string? priorityEffect = null;
            if (task.TaskTypeId is 2 or 3 && task.Priority.HasValue)
            {
                const double decayFactor = 0.1;
                var priorityIndex = task.Priority.Value - 1;
                priorityMultiplier = Math.Exp(-decayFactor * priorityIndex);
                priorityEffect = $"Priority {task.Priority} reduces XP by {Math.Round((1 - priorityMultiplier.Value) * 100, 1)}%";
            }

            // Calculate potential bonus XP from equipped items
            double? bonusXp = null;
            string? bonusItemName = null;
            if (task.UserTree?.UserId != null)
            {
                try
                {
                    var equippedItem = await bagRepository.GetEquippedItemAsync(task.UserTree.UserId.Value, ItemType.XpBoostTree);
                    if (equippedItem?.Item?.ItemDetail?.Effect != null && 
                        double.TryParse(equippedItem.Item.ItemDetail.Effect, out var effectPercent) && 
                        effectPercent > 0)
                    {
                        bonusXp = Math.Round(baseXp * (effectPercent / 100), 2);
                        bonusItemName = equippedItem.Item.Name;
                    }
                }
                catch (Exception)
                {
                    // Ignore item errors when calculating potential XP
                }
            }

            return new TaskXpInfoDto
            {
                TaskId = task.TaskId,
                TaskName = task.TaskName,
                BaseXp = baseXp,
                BonusXp = bonusXp,
                BonusItemName = bonusItemName,
                TotalXp = bonusXp.HasValue ? baseXp + bonusXp.Value : baseXp,
                ActivityType = ActivityType.TaskXp,
                CreatedAt = DateTime.UtcNow,
                Priority = task.Priority,
                PriorityMultiplier = priorityMultiplier,
                OriginalBaseXp = originalBaseXp,
                PriorityEffect = priorityEffect
            };
        }

        // If task is completed, get the actual XP earned
        var latestXpLog = xpLog.OrderByDescending(x => x.CreatedAt).First();
        var actualBaseXp = latestXpLog.XpAmount;

        // Calculate priority effect for completed task
        double? actualPriorityMultiplier = null;
        string? actualPriorityEffect = null;
        if (task.TaskTypeId is 2 or 3 && task.Priority.HasValue)
        {
            const double decayFactor = 0.1;
            var priorityIndex = task.Priority.Value - 1;
            actualPriorityMultiplier = Math.Exp(-decayFactor * priorityIndex);
            actualPriorityEffect = $"Priority {task.Priority} reduced XP by {Math.Round((1 - actualPriorityMultiplier.Value) * 100, 1)}%";
        }

        // Try to get bonus XP information from the task completion
        double? actualBonusXp = null;
        string? actualBonusItemName = null;
        if (task.UserTree?.UserId != null)
        {
            try
            {
                var equippedItem = await bagRepository.GetEquippedItemAsync(task.UserTree.UserId.Value, ItemType.XpBoostTree);
                if (equippedItem?.Item?.ItemDetail?.Effect != null && 
                    double.TryParse(equippedItem.Item.ItemDetail.Effect, out var effectPercent) && 
                    effectPercent > 0)
                {
                    actualBonusXp = Math.Round(actualBaseXp * (effectPercent / 100), 2);
                    actualBonusItemName = equippedItem.Item.Name;
                }
            }
            catch (Exception)
            {
                // Ignore item errors when getting actual XP
            }
        }

        return new TaskXpInfoDto
        {
            TaskId = task.TaskId,
            TaskName = task.TaskName,
            BaseXp = actualBaseXp,
            BonusXp = actualBonusXp,
            BonusItemName = actualBonusItemName,
            TotalXp = actualBonusXp.HasValue ? actualBaseXp + actualBonusXp.Value : actualBaseXp,
            ActivityType = latestXpLog.ActivityType,
            CreatedAt = latestXpLog.CreatedAt,
            Priority = task.Priority,
            PriorityMultiplier = actualPriorityMultiplier,
            OriginalBaseXp = actualBaseXp / (actualPriorityMultiplier ?? 1),
            PriorityEffect = actualPriorityEffect
        };
    }

    public async Task<TaskDto?> GetActiveTaskByUserIdAsync(int userId)
    {
        var task = await taskRepository.GetUserTaskInProgressAsync(userId);
        if (task == null) return null;

        var taskDto = mapper.Map<TaskDto>(task);
        var remainingSeconds = CalculateRemainingSeconds(task);
        taskDto.RemainingTime = StringHelper.FormatSecondsToTime(remainingSeconds);
        var accumulatedSeconds = (int)((task.AccumulatedTime ?? 0) * 60);
        taskDto.AccumulatedTime = StringHelper.FormatSecondsToTime(accumulatedSeconds);

        return taskDto;
    }
}