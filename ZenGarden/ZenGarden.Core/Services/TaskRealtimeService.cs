using Microsoft.AspNetCore.SignalR;
using ZenGarden.Core.Hubs;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Services;

public class TaskRealtimeService
{
    private readonly IHubContext<TaskHub> _hubContext;
    private readonly ITaskRepository _taskRepository;
    public TaskRealtimeService(IHubContext<TaskHub> hubContext, ITaskRepository taskRepository)
    {
        _hubContext = hubContext;
        _taskRepository = taskRepository;
    }

    public async Task NotifyTaskUpdated(TaskDto task)
    {
        // Notify specific task group
        await _hubContext.Clients.Group($"Task_{task.TaskId}")
            .SendAsync("TaskUpdated", task);
        var userid = await _taskRepository.GetUserIdByTaskIdAsync(task.TaskId)
                   ?? throw new InvalidOperationException($"User ID not found for Task ID {task.TaskId}");
        var userTreeId = await _taskRepository.GetUserTreeIdByTaskIdAsync(task.TaskId);
        // Notify user's tasks group
        await _hubContext.Clients.Group($"UserTasks_{userid}")
            .SendAsync("UserTaskUpdated", task);

        // Notify tree's tasks group if tree exists
        if (userTreeId != null)
        {
            await _hubContext.Clients.Group($"TreeTasks_{userTreeId}")
                .SendAsync("TreeTaskUpdated", task);
        }
    }

    public async Task NotifyTaskCreated(TaskDto task)
    {
        // Notify specific task group
        await _hubContext.Clients.Group($"Task_{task.TaskId}")
            .SendAsync("TaskCreated", task);

        // Skip User ID validation for challenge tasks (TaskTypeId = 4)
        if (task.TaskTypeName == "Challenge")
        {
            return;
        }

        var userid = await _taskRepository.GetUserIdByTaskIdAsync(task.TaskId)
                   ?? throw new InvalidOperationException($"User ID not found for Task ID {task.TaskId}");
        var userTreeId = await _taskRepository.GetUserTreeIdByTaskIdAsync(task.TaskId);

        // Notify user's tasks group
        await _hubContext.Clients.Group($"UserTasks_{userid}")
            .SendAsync("UserTaskCreated", task);

        // Notify tree's tasks group if tree exists
        if (userTreeId != null)
        {
            await _hubContext.Clients.Group($"TreeTasks_{userTreeId}")
                .SendAsync("TreeTaskCreated", task);
        }
    }

    public async Task NotifyTaskDeleted(int taskId, int? userId, int? treeId)
    {
        // Notify specific task group
        await _hubContext.Clients.Group($"Task_{taskId}")
            .SendAsync("TaskDeleted", taskId);

        var userid = await _taskRepository.GetUserIdByTaskIdAsync(taskId)
                   ?? throw new InvalidOperationException($"User ID not found for Task ID {taskId}");
        var userTreeId = await _taskRepository.GetUserTreeIdByTaskIdAsync(taskId);

        // Notify user's tasks group
        await _hubContext.Clients.Group($"UserTasks_{userid}")
            .SendAsync("UserTaskDeleted", taskId);

        // Notify tree's tasks group if tree exists
        if (userTreeId != null)
        {
            await _hubContext.Clients.Group($"TreeTasks_{userTreeId}")
                .SendAsync("TreeTaskDeleted", taskId);
        }
    }

    public async Task NotifyTaskStatusChanged(TaskDto task)
    {
        // Notify specific task group
        await _hubContext.Clients.Group($"Task_{task.TaskId}")
            .SendAsync("TaskStatusChanged", task);

        var userid = await _taskRepository.GetUserIdByTaskIdAsync(task.TaskId)
                   ?? throw new InvalidOperationException($"User ID not found for Task ID {task.TaskId}");
        var userTreeId = await _taskRepository.GetUserTreeIdByTaskIdAsync(task.TaskId);

        // Notify user's tasks group
        await _hubContext.Clients.Group($"UserTasks_{userid}")
            .SendAsync("UserTaskStatusChanged", task);

        // Notify tree's tasks group if tree exists
        if (userTreeId != null)
        {
            await _hubContext.Clients.Group($"TreeTasks_{userTreeId}")
                .SendAsync("TreeTaskStatusChanged", task);
        }
    }
} 