using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class PurchaseHistoryRepository(ZenGardenContext context)
    : GenericRepository<PurchaseHistory>(context), IPurchaseHistoryRepository
{
    public async Task<int> CountPurchaseThisMonth(int userId, int itemId, DateTime startOfMonth)
    {
        return await context.PurchaseHistory
            .Where(ph => ph.UserId == userId
                         && ph.ItemId == itemId
                         && ph.CreatedAt >= startOfMonth)
            .CountAsync();
    }
}