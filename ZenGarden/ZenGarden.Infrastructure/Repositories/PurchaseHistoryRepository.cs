using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class PurchaseHistoryRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<PurchaseHistory>(context, redisService), IPurchaseHistoryRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<int> CountPurchaseThisMonth(int userId, int itemId, DateTime startOfMonth)
    {
        return await _context.PurchaseHistory
            .Where(ph => ph.UserId == userId
                         && ph.ItemId == itemId
                         && ph.CreatedAt >= startOfMonth)
            .CountAsync();
    }
}