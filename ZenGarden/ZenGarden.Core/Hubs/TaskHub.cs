using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace ZenGarden.Core.Hubs;

public class TaskHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> Connections = new();

    public override Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return base.OnConnectedAsync();
        
        Connections[userId] = Context.ConnectionId;
        Console.WriteLine($"✅ User {userId} connected to TaskHub with {Context.ConnectionId}");

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Connections.ContainsKey(userId)) return base.OnDisconnectedAsync(exception);
        
        Connections.TryRemove(userId, out _);
        Console.WriteLine($"❌ User {userId} disconnected from TaskHub");

        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinTaskGroup(string taskId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Task_{taskId}");
    }

    public async Task LeaveTaskGroup(string taskId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Task_{taskId}");
    }

    public async Task JoinUserTasksGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"UserTasks_{userId}");
    }

    public async Task LeaveUserTasksGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"UserTasks_{userId}");
    }

    public async Task JoinTreeTasksGroup(string treeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"TreeTasks_{treeId}");
    }

    public async Task LeaveTreeTasksGroup(string treeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"TreeTasks_{treeId}");
    }
} 