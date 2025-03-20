using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class BagRepository(ZenGardenContext context) : GenericRepository<Bag>(context), IBagRepository
{
    public async Task<Bag?> GetByUserIdAsync(int userId)
    {
        return await context.Bag.FirstOrDefaultAsync(b => b.UserId == userId);
    }
}