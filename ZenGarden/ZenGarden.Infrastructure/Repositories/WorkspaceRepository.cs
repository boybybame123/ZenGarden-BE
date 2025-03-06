using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class WorkspaceRepository(ZenGardenContext context)
    : GenericRepository<Workspace>(context), IWorkspaceRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Workspace?> GetByUserIdAsync(int userId)
    {
        return await _context.Workspace
            .Include(w => w.Tasks)
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }
}