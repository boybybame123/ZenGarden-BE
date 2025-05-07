using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TreeXpLogRepository(ZenGardenContext context)
    : GenericRepository<TreeXpLog>(context), ITreeXpLogRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<TreeXpLog>> GetTreeXpLogByTaskIdAsync(int taskId)
    {
        return await _context.TreeXpLog
            .Where(x => x.TaskId == taskId)
            .ToListAsync();
    }

    public async Task<TreeXpLog?> GetLatestTreeXpLogByUserTreeIdAsync(int userTreeId)
    {
        return await _context.TreeXpLog
            .Include(log => log.Tasks)
            .Where(log => log.Tasks != null && log.Tasks.UserTreeId == userTreeId)
            .OrderByDescending(log => log.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TreeXpLog>> GetTreeXpLogByUserIdAsync(int userId)
    {
        return await _context.TreeXpLog
            .Include(log => log.Tasks)
            .ThenInclude(task => task!.UserTree)
            .Where(log => log.Tasks != null && 
                         log.Tasks.UserTree != null && 
                         log.Tasks.UserTree.UserId == userId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<TreeXpLog>> GetTreeXpLogByUserTreeIdAsync(int userTreeId)
    {
        return await _context.TreeXpLog
            .Include(log => log.Tasks)
            .Where(log => log.Tasks != null && log.Tasks.UserTreeId == userTreeId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();
    }
}