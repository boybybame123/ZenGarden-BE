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
    
    public async Task<TaskDto?> GetTaskByUserTreeIdAsync(int userTreeId)
    {
        var task = await taskRepository.GetTaskByUserTreeIdAsync(userTreeId)
                   ?? throw new KeyNotFoundException($"Task with UserTree ID {userTreeId} not found.");

        return mapper.Map<TaskDto>(task);
    }

    public async Task<TaskDto> CreateTaskWithSuggestedMethodAsync(CreateTaskDto dto)
    {
        var selectedMethod = dto.FocusMethodId.HasValue
            ? mapper.Map<FocusMethodDto>(await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value))
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
        var existingTask = await taskRepository.GetByIdAsync(updateTaskDto.TaskId);
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

        if (updateTaskDto.TotalDuration.HasValue)
            existingTask.TotalDuration = updateTaskDto.TotalDuration.Value;
        
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

        var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                       ?? throw new KeyNotFoundException("XP configuration not found for this task.");
        
        // if (task.StartedAt == null)
        //     throw new InvalidOperationException("Task must have a start time to be completed.");
        //
        // if (DateTime.UtcNow - task.StartedAt < TimeSpan.FromMinutes(task.TotalDuration ?? 0))
        //     throw new InvalidOperationException("Task cannot be completed before the required duration has passed.");
        
        var workDuration = task.WorkDuration ?? 0;
        if (workDuration <= 0)
            throw new InvalidOperationException("Invalid WorkDuration for task completion.");

        var standardDuration = task.FocusMethod?.MinDuration ?? 25;
        var xpEarned = (workDuration / (double)standardDuration) * xpConfig.BaseXp * xpConfig.Multiplier;

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
    
            await userTreeService.CheckAndSetMaxLevelAsync(task.UserTree);
        }

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

    public async Task<double> CalculateTaskXpAsync(int taskId)
    {
        var task = await taskRepository.GetTaskWithDetailsAsync(taskId)
                   ?? throw new KeyNotFoundException("Task not found.");

        if (task.FocusMethodId == null)
            throw new InvalidOperationException("Task does not have an assigned FocusMethod.");

        var xpConfig = await xpConfigRepository.GetXpConfigAsync(task.TaskTypeId, task.FocusMethodId.Value)
                       ?? throw new KeyNotFoundException("XP configuration not found.");

        var standardDuration = task.FocusMethod?.MinDuration ?? 25;
        return (task.WorkDuration ?? 25) / (double)standardDuration * xpConfig.BaseXp * xpConfig.Multiplier;
    }
}