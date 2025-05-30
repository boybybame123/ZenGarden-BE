using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TaskRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<Tasks>(context, redisService), ITaskRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Tasks?> GetUserTaskInProgressAsync(int userId)
    {
        return await _context.Tasks
            .Where(t => t.UserTree.UserId == userId &&
                        (t.Status == TasksStatus.InProgress || t.Status == TasksStatus.Paused))
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
            .AsSplitQuery()
            .Include(t => t.TaskType)
            .Include(t => t.FocusMethod)
            .Include(t => t.UserTree)
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetOverdueTasksAsync()
    {
        return await _context.Tasks
            .Where(t => (t.Status == TasksStatus.InProgress ||
                         t.Status == TasksStatus.Paused ||
                         t.Status == TasksStatus.NotStarted)
                        && t.EndDate < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetClonedTasksByUserChallengeAsync(int userId, int challengeId)
    {
        return await _context.Tasks
            .Where(t => t.CloneFromTaskId != null && 
                        t.UserTree.UserId == userId &&
                        _context.ChallengeTask.Any(ct => 
                            ct.TaskId == t.CloneFromTaskId && 
                            ct.ChallengeId == challengeId))
            .GroupBy(t => t.CloneFromTaskId)
            .Select(g => g.First())
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
        var allTasks = await _context.Tasks
            .Where(t => t.UserTreeId == userTreeId)
            .Select(t => t.Priority)
            .ToListAsync();
        var maxPriority = allTasks
            .Where(p => p.HasValue)
            .DefaultIfEmpty(0)
            .Max() ?? 0;
        return maxPriority + 1;
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

    public async Task<List<Tasks>> GetReorderableTasksByIdsAsync(List<int> taskIds)
    {
        return await _context.Tasks
            .Include(t => t.UserTree)
            .Where(t => taskIds.Contains(t.TaskId) &&
                        (t.TaskTypeId == 2 || t.TaskTypeId == 3) &&
                        t.Status != TasksStatus.InProgress && t.Status != TasksStatus.Paused)
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetTasksByStartDateTimeMatchingAsync(DateTime currentTime)
    {
        return await _context.Tasks
            .Include(t => t.UserTree)
            .Where(t =>
                t.StartDate.HasValue &&
                t.StartDate.Value.Year == currentTime.Year &&
                t.StartDate.Value.Month == currentTime.Month &&
                t.StartDate.Value.Day == currentTime.Day &&
                t.StartDate.Value.Hour == currentTime.Hour &&
                t.StartDate.Value.Minute == currentTime.Minute &&
                t.Status == TasksStatus.NotStarted)
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetTasksWithPassedStartDateNotStartedAsync(
        DateTime currentTime)
    {
        return await _context.Tasks
            .Include(t => t.UserTree)
            .Where(t =>
                t.StartDate.HasValue &&
                t.StartDate.Value < currentTime &&
                t.EndDate.HasValue &&
                t.EndDate.Value > currentTime &&
                t.Status == TasksStatus.NotStarted)
            .ToListAsync();
    }

    public async Task<List<Tasks>> GetTasksWithEndDateMatchingAsync(
        DateTime targetDate,
        bool onlyMatchDay)
    {
        var query = _context.Tasks
            .Include(t => t.UserTree)
            .Where(t =>
                t.EndDate.HasValue &&
                t.Status != TasksStatus.Completed &&
                t.Status != TasksStatus.Canceled &&
                t.Status != TasksStatus.Overdue);

        if (onlyMatchDay)
            // Trùng ngày (dùng cho thông báo trước 1 ngày)
            query = query.Where(t =>
                t.EndDate != null &&
                t.EndDate.Value.Year == targetDate.Year &&
                t.EndDate.Value.Month == targetDate.Month &&
                t.EndDate.Value.Day == targetDate.Day);
        else
            // Trùng chính xác đến phút (dùng cho thông báo trước 5 phút)
            query = query.Where(t =>
                t.EndDate != null &&
                t.EndDate.Value.Year == targetDate.Year &&
                t.EndDate.Value.Month == targetDate.Month &&
                t.EndDate.Value.Day == targetDate.Day &&
                t.EndDate.Value.Hour == targetDate.Hour &&
                t.EndDate.Value.Minute == targetDate.Minute);

        return await query.ToListAsync();
    }

    public async Task<int?> GetUserTreeIdByTaskIdAsync(int taskId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        return task?.UserTreeId;

    }


    
}