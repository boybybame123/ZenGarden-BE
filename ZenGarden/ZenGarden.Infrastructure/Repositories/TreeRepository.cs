using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TreeRepository(ZenGardenContext context) : GenericRepository<Tree>(context), ITreeRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<int>> GetAllTreeIdsAsync()
    {
        return await _context.Tree.Select(t => t.TreeId).ToListAsync();
    }
}