using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class TaskService(ITaskRepository taskRepository, 
    IFocusMethodRepository focusMethodRepository,
    ITaskFocusRepository taskFocusRepository,
    IWorkspaceRepository workspaceRepository,
    IUnitOfWork unitOfWork, 
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

    public async Task<Tasks> CreateTaskAsync(CreateTaskDto dto)
{
    var focusMethod = dto.FocusMethodId > 0
        ? await focusMethodRepository.GetByIdAsync(dto.FocusMethodId)
        : await focusMethodRepository.GetRecommendedMethodAsync(dto.TaskName, dto.TaskDescription);

    if (focusMethod == null)
    {
        var availableMethods = await focusMethodRepository.GetAllAsync();
        throw new InvalidOperationException($"No recommended method found. Available options: {string.Join(", ", availableMethods.Select(m => m.Name))}");
    }

    var task = new Tasks
    {
        UserId = dto.UserId,
        Status = TasksStatus.InProgress,
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
        FocusMethodId = focusMethod.FocusMethodId,
        FocusMethod = focusMethod,
        SuggestedDuration = focusMethod.DefaultDuration ?? 25,
        SuggestedBreak = focusMethod.DefaultBreak ?? 5
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
}