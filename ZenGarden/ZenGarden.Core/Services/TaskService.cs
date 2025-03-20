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
    ITreeXpLogRepository treeXpLogRepository,
    IUserTreeService userTreeService,
    IUserXpLogRepository userXpLogRepository,
    IUserXpLogService userXpLogService,
    IFocusMethodService focusMethodService,
    IXpConfigService xpConfigService,
    IMapper mapper) : ITaskService
{
    public async Task<List<TaskDto>> GetAllTaskAsync()
    {
        var tasks = await taskRepository.GetAllWithDetailsAsync();
        return mapper.Map<List<TaskDto>>(tasks);
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int taskId)
    {
        var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        return mapper.Map<TaskDto>(task);
    }
    
    public async Task<List<TaskDto>> GetTaskByUserIdAsync(int userId)
    {
        var tasks = await taskRepository.GetTasksByUserIdAsync(userId);
    
        if (tasks == null || tasks.Count == 0)
        {
            throw new KeyNotFoundException($"Tasks with UserTree ID {userId} not found.");
        }

        return mapper.Map<List<TaskDto>>(tasks);
    }

    public async Task<List<TaskDto>> GetTaskByUserTreeIdAsync(int userTreeId)
    {
        var tasks = await taskRepository.GetTasksByUserTreeIdAsync(userTreeId);
    
        if (tasks == null || tasks.Count == 0)
        {
            throw new KeyNotFoundException($"Tasks with UserTree ID {userTreeId} not found.");
        }

        return mapper.Map<List<TaskDto>>(tasks);
    }

    public async Task<TaskDto> CreateTaskWithSuggestedMethodAsync(CreateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TaskName) || string.IsNullOrWhiteSpace(dto.TaskDescription))
            throw new ArgumentException("Task name and description cannot be empty.");

        var selectedMethod = dto.FocusMethodId.HasValue
            ? await focusMethodRepository.GetDtoByIdAsync(dto.FocusMethodId.Value)
            : await focusMethodService.SuggestFocusMethodAsync(new SuggestFocusMethodDto
            {
                TaskName = dto.TaskName,
                TaskDescription = dto.TaskDescription,
                TotalDuration = dto.TotalDuration,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            });

        if (selectedMethod == null)
            throw new InvalidOperationException("No valid focus method found.");

        var existingTaskType = await taskTypeRepository.GetByIdAsync(dto.TaskTypeId);
        if (existingTaskType == null)
            throw new KeyNotFoundException("TaskType not found.");

        if (!dto.TotalDuration.HasValue)
            throw new InvalidOperationException("TotalDuration is required but was null.");

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            await xpConfigService.EnsureXpConfigExists(
                selectedMethod.FocusMethodId,
                dto.TaskTypeId,
                dto.TotalDuration.Value
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
            await transaction.CommitAsync();
            return mapper.Map<TaskDto>(newTask);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task UpdateTaskAsync(UpdateTaskDto updateTaskDto)
    {
        var existingTask = await taskRepository.GetByIdAsync(updateTaskDto.TaskId);
        if (existingTask == null)
            throw new KeyNotFoundException($"Task with ID {updateTaskDto.TaskId} not found.");

        var isUpdated = false;

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskName) && existingTask.TaskName != updateTaskDto.TaskName)
        {
            existingTask.TaskName = updateTaskDto.TaskName;
            isUpdated = true;
        }

        if (!string.IsNullOrWhiteSpace(updateTaskDto.TaskDescription) &&
            existingTask.TaskDescription != updateTaskDto.TaskDescription)
        {
            existingTask.TaskDescription = updateTaskDto.TaskDescription;
            isUpdated = true;
        }

        if (updateTaskDto.TotalDuration.HasValue && existingTask.TotalDuration != updateTaskDto.TotalDuration.Value)
        {
            existingTask.TotalDuration = updateTaskDto.TotalDuration.Value;
            isUpdated = true;
        }

        if (isUpdated)
        {
            existingTask.UpdatedAt = DateTime.UtcNow;
            taskRepository.Update(existingTask);
            if (await unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to update task.");
        }
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

        if (task.Status != TasksStatus.NotStarted)
            throw new InvalidOperationException("Only not started tasks can be started.");

        var existingInProgressTask = await taskRepository.GetUserTaskInProgressAsync(userId);
        if (existingInProgressTask != null)
            throw new InvalidOperationException(
                $"You already have a task in progress (Task ID: {existingInProgressTask.TaskId}). Please complete it first.");

        var today = DateTime.UtcNow.Date;
        var checkInLog = await userXpLogRepository.GetUserCheckInLogAsync(userId, today);

        if (checkInLog == null) await userXpLogService.CheckInAndGetXpAsync(userId);

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

        // if (task.StartedAt == null)
        //     throw new InvalidOperationException("Task must have a start time to be completed.");
        //
        // if (DateTime.UtcNow - task.StartedAt < TimeSpan.FromMinutes(task.TotalDuration ?? 0))
        //     throw new InvalidOperationException("Task cannot be completed before the required duration has passed.");

        var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                       ?? throw new KeyNotFoundException("XP configuration not found for this task.");

        if (xpConfig.BaseXp <= 0)
            throw new InvalidOperationException("Invalid BaseXp value in XP configuration.");

        var xpEarned = xpConfig.BaseXp * xpConfig.XpMultiplier;

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            if (xpEarned > 0)
            {
                task.UserTree.TotalXp += xpEarned;

                var xpLog = new TreeXpLog
                {
                    TaskId = task.TaskId,
                    ActivityType = ActivityType.TaskXp,
                    XpAmount = xpEarned,
                    CreatedAt = DateTime.UtcNow
                };
                await treeXpLogRepository.CreateAsync(xpLog);
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

        await userTreeService.CheckAndSetMaxLevelAsync(task.UserTree);
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
}