using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TaskRepository(ZenGardenContext context) : GenericRepository<Tasks>(context), ITaskRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Tasks?> GetUserTaskInProgressAsync(int userId)
    {
        return await _context.Tasks
            .Where(t => t.UserTree.UserId == userId && t.Status == TasksStatus.InProgress)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Tasks>> GetTasksByUserTreeIdAsync(int userTreeId)
    {
        return await _context.Tasks
            .Include(t => t.UserTree)
            .Include(t => t.FocusMethod)
            .Include(t => t.TaskType)
            .Where(t => t.UserTreeId == userTreeId)
            .ToListAsync();
    }
    
    public async Task<List<Tasks>> GetTasksByUserIdAsync(int userId)
    {
        return await _context.Tasks
            .Include(t => t.UserTree)
            .Include(t => t.FocusMethod)
            .Include(t => t.TaskType)
            .Where(t => t.UserTree.UserId == userId)
            .ToListAsync();
    }

    public async Task<Tasks?> GetTaskWithDetailsAsync(int taskId)
    {
        return await _context.Tasks
            .Include(t => t.UserTree)
            .Include(t => t.FocusMethod)
            .Include(t => t.TaskType)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);
    }

    public async Task<List<Tasks>> GetAllWithDetailsAsync()
    {
        return await _context.Tasks
            .Include(t => t.TaskType)
            .Include(t => t.FocusMethod)
            .Include(t => t.UserTree)
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetOverdueTasksAsync()
    {
        return await _context.Tasks
            .Where(t => t.Status == TasksStatus.InProgress && t.EndDate < DateTime.UtcNow)
            .ToListAsync();
    }
}