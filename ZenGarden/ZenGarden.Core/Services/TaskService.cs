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
    ITaskFocusRepository taskFocusRepository,
    IWorkspaceRepository workspaceRepository,
    IUnitOfWork unitOfWork,
    IUserTreeRepository userTreeRepository,
    ITreeXpLogRepository treeXpLogRepository,
    ITreeLevelConfigRepository treeLevelConfigRepository,
    IMapper mapper) : ITaskService
{
    public async Task<List<TaskDto>> GetAllTaskAsync()
    {
        var tasks = await taskRepository.GetAllAsync();
        return mapper.Map<List<TaskDto>>(tasks);
    }

    public async Task<Tasks?> GetTaskByIdAsync(int taskId)
    {
        return await taskRepository.GetByIdAsync(taskId)
               ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");
    }

    public async Task<Tasks> CreateTaskAsync(FinalizeTaskDto dto)
    {
        var suggestedMethod = await focusMethodRepository.GetRecommendedMethodAsync(dto.TaskName, dto.TaskDescription);

        var selectedFocusMethod = suggestedMethod?.FocusMethodId == dto.FocusMethodId
            ? suggestedMethod
            : await focusMethodRepository.GetByIdAsync(dto.FocusMethodId)
              ?? throw new InvalidOperationException("Invalid Focus Method selected.");

        if (dto.Duration < selectedFocusMethod.MinDuration || dto.Duration > selectedFocusMethod.MaxDuration)
            throw new ArgumentException(
                $"Duration must be between {selectedFocusMethod.MinDuration} and {selectedFocusMethod.MaxDuration}");


        var task = new Tasks
        {
            UserId = dto.UserId,
            Status = TasksStatus.NotStarted,
            Duration = dto.Duration,
            BaseXp = dto.BaseXp,
            TaskName = dto.TaskName,
            TaskDescription = dto.TaskDescription,
            Type = dto.Type
        };

        await taskRepository.CreateAsync(task);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create task.");

        var taskFocusSetting = new TaskFocusSetting
        {
            TaskId = task.TaskId,
            Task = task,
            FocusMethodId = selectedFocusMethod.FocusMethodId,
            FocusMethod = selectedFocusMethod,
            SuggestedDuration = selectedFocusMethod.DefaultDuration ?? 25,
            SuggestedBreak = selectedFocusMethod.DefaultBreak ?? 5,
            CustomDuration = dto.CustomDuration,
            CustomBreak = dto.CustomBreak
        };

        await taskFocusRepository.CreateAsync(taskFocusSetting);

        var workspace = await workspaceRepository.GetByUserIdAsync(dto.UserId);
        if (workspace != null)
        {
            workspace.Tasks.Add(task);
            workspace.UpdatedAt = DateTime.UtcNow;
            workspaceRepository.Update(workspace);
        }
        else
        {
            workspace = new Workspace
            {
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Tasks = new List<Tasks> { task }
            };
            await workspaceRepository.CreateAsync(workspace);
        }

        await unitOfWork.CommitAsync();
        return task;
    }


    public async Task UpdateTaskAsync(TaskDto task)
    {
        var updateTask = await GetTaskByIdAsync(task.TaskId);
        if (updateTask == null)
            throw new KeyNotFoundException($"Task with ID {task.TaskId} not found.");

        mapper.Map(task, updateTask);

        taskRepository.Update(updateTask);
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

    public async Task<FocusMethod?> GetSuggestedFocusMethodsAsync(string taskName, string? taskDescription)
    {
        return await focusMethodRepository.GetRecommendedMethodAsync(taskName, taskDescription);
    }

    public async Task StartTaskAsync(int taskId)
    {
        var task = await taskRepository.GetByIdAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (!task.UserId.HasValue)
            throw new InvalidOperationException("Task must be associated with a valid user.");

        var existingTaskInProgress = await taskRepository.GetUserTaskInProgressAsync(task.UserId.Value);

        if (existingTaskInProgress != null)
            throw new InvalidOperationException("You must complete your current task before starting a new one.");

        task.Status = TasksStatus.InProgress;
        taskRepository.Update(task);

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to start the task.");
    }

    public async Task CompleteTaskAsync(int taskId)
    {
        var task = await taskRepository.GetByIdAsync(taskId)
                   ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        if (task.Status != TasksStatus.InProgress)
            throw new InvalidOperationException("Only in-progress tasks can be completed.");

        task.Status = TasksStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        taskRepository.Update(task);

        if (task.UserTreeId.HasValue)
        {
            var userTree = await userTreeRepository.GetByIdAsync(task.UserTreeId.Value);
            if (userTree != null)
            {
                var xpGained = task.Type switch
                {
                    TaskType.InWorkspace => (int)(task.BaseXp * 1.2),
                    TaskType.External => (int)(task.BaseXp * 0.8),
                    _ => task.BaseXp
                };

                userTree.TotalXp += xpGained;

                await treeXpLogRepository.CreateAsync(new TreeXpLog
                {
                    UserTreeId = userTree.UserTreeId,
                    TaskId = task.TaskId,
                    ActivityType = ActivityType.TASK_XP,
                    XpAmount = xpGained,
                    UserTree = userTree,
                    Task = task
                });

                var nextLevel = await treeLevelConfigRepository.GetByIdAsync(userTree.TreeLevel + 1);
                if (nextLevel != null && userTree.TotalXp >= nextLevel.XpRequired)
                {
                    userTree.TreeLevel += 1;
                    userTree.TotalXp = 0;
                }

                userTree.UpdatedAt = DateTime.UtcNow;
                userTreeRepository.Update(userTree);
            }
        }

        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to complete the task.");
    }
}