using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TreeXpConfigRepository(ZenGardenContext context)
    : GenericRepository<TreeXpConfig>(context), ITreeXpConfigRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<TreeXpConfig?> GetNextLevelConfigAsync(int currentLevelId)
    {
        return await _context.TreeXpConfig
            .Where(c => c.LevelId > currentLevelId)
            .OrderBy(c => c.LevelId)
            .FirstOrDefaultAsync();
    }

    public async Task<TreeXpConfig?> GetMaxLevelConfigAsync()
    {
        return await _context.TreeXpConfig
            .OrderByDescending(c => c.LevelId)
            .FirstOrDefaultAsync();
    }
}