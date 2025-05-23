using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TreeRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<Tree>(context, redisService), ITreeRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<int>> GetAllTreeIdsAsync()
    {
        return await _context.Tree.Select(t => t.TreeId).ToListAsync();
    }

    public async Task<int?> GetRandomFinalTreeIdAsync()
    {
        var finalTreeIds = await _context.Tree
            .Where(t => t.IsActive)
            .Select(t => t.TreeId)
            .ToListAsync();

        return finalTreeIds.Count != 0 ? finalTreeIds[new Random().Next(finalTreeIds.Count)] : null;
    }
}