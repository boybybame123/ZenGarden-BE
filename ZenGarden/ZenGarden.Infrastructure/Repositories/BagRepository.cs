using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class BagRepository(ZenGardenContext context) : GenericRepository<Bag>(context), IBagRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Bag?> GetByUserIdAsync(int userId)
    {
        return await _context.Bag.FirstOrDefaultAsync(b => b.UserId == userId);
    }
}