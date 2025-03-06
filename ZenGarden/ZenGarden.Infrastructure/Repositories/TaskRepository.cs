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
            .Where(t => t.UserId == userId && t.Status == TasksStatus.InProgress)
            .FirstOrDefaultAsync();
    }
}