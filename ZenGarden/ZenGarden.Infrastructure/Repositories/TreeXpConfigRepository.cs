using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TreeXpConfigRepository(ZenGardenContext context)
    : GenericRepository<TreeXpConfig>(context), ITreeXpConfigRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<int> GetMaxXpThresholdAsync()
    {
        return await _context.TreeXpConfig
            .OrderByDescending(x => x.XpThreshold)
            .Select(x => x.XpThreshold)
            .FirstOrDefaultAsync();
    }
}