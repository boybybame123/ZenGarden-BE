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

    public async Task<List<Tasks>> GetClonedTasksByUserChallengeAsync(int userId, int challengeId)
    {
        return await (from t in _context.Tasks
                join original in _context.Tasks on t.CloneFromTaskId equals original.TaskId
                join ct in _context.ChallengeTask on original.TaskId equals ct.TaskId
                join uc in _context.UserChallenges on ct.ChallengeId equals uc.ChallengeId
                where uc.UserId == userId
                      && uc.ChallengeId == challengeId
                      && t.CloneFromTaskId != null
                select t)
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetAllTasksByChallengeIdAsync(int challengeId)
    {
        return await (from t in _context.Tasks
                join ct in _context.ChallengeTask on t.TaskId equals ct.TaskId
                where ct.ChallengeId == challengeId
                select t)
            .Union(
                from cloned in _context.Tasks
                join original in _context.Tasks on cloned.CloneFromTaskId equals original.TaskId
                join ct in _context.ChallengeTask on original.TaskId equals ct.TaskId
                where ct.ChallengeId == challengeId
                select cloned
            )
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetTasksInProgressBeforeAsync(DateTime thresholdTime)
    {
        return await _context.Tasks
            .Where(t => t.Status == TasksStatus.InProgress &&
                        t.StartedAt.HasValue &&
                        t.StartedAt.Value < thresholdTime)
            .ToListAsync();
    }

    public async Task<Tasks?> GetActiveTaskByUserTreeIdAsync(int userTreeId)
    {
        return await _context.Tasks
            .Where(t => t.UserTreeId == userTreeId
                        && (t.Status == TasksStatus.InProgress || t.Status == TasksStatus.Paused))
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Tasks>> GetDailyTasksAsync()
    {
        return await _context.Tasks
            .Where(t => t.TaskType.TaskTypeName.ToLower() == "daily")
            .ToListAsync();
    }

    public async Task<int> GetCompletedTasksAsync(int userId, int challengeId)
    {
        return await _context.Tasks
            .Where(t =>
                t.CloneFromTaskId != null &&
                t.UserTree.UserId == userId &&
                t.Status == TasksStatus.Completed &&
                _context.ChallengeTask.Any(ct =>
                    ct.TaskId == t.CloneFromTaskId && ct.ChallengeId == challengeId)
            )
            .CountAsync();
    }

    public async Task<int> GetTotalCloneTasksAsync(int userId, int challengeId)
    {
        return await _context.Tasks
            .Where(t =>
                t.CloneFromTaskId != null &&
                t.UserTree.UserId == userId &&
                _context.ChallengeTask.Any(ct =>
                    ct.TaskId == t.CloneFromTaskId && ct.ChallengeId == challengeId)
            )
            .CountAsync();
    }

    public async Task<int> GetNextPriorityForTreeAsync(int userTreeId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.UserTreeId == userTreeId &&
                        (t.Status == TasksStatus.NotStarted ||
                         t.Status == TasksStatus.InProgress ||
                         t.Status == TasksStatus.Paused))
            .ToListAsync();

        var maxPriority = tasks
            .Select(t => t.Priority)
            .Max() ?? 0;

        return maxPriority + 1;
    }

    public async Task<List<Tasks>> GetTasksByIdsAsync(List<int> taskIds)
    {
        return await _context.Tasks
            .Include(t => t.UserTree)
            .Where(t => taskIds.Contains(t.TaskId))
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetActiveTasksByUserTreeIdAsync(int userTreeId)
    {
        return await _context.Tasks
            .Where(t => t.UserTreeId == userTreeId &&
                        (t.TaskTypeId == 2 || t.TaskTypeId == 3) &&
                        t.Status != TasksStatus.Completed &&
                        t.Status != TasksStatus.Overdue &&
                        t.Status != TasksStatus.Canceled)
            .ToListAsync();
    }
    public async Task<int?> GetUserIdByTaskIdAsync(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.UserTree)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        return task?.UserTree?.UserId;
    }
    
}