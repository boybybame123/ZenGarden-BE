using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TradeHistoryRepository(ZenGardenContext context)
    : GenericRepository<TradeHistory>(context), ITradeHistoryRepository
{

    public async Task<bool> IsTreeInPendingTradeAsync(int userTreeId)
    {
        return await context.TradeHistory
            .AnyAsync(t => (t.TreeOwnerAid== userTreeId )
                        && t.Status == TradeStatus.Pending);
    }

}