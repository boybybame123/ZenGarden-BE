using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TradeHistoryRepository(ZenGardenContext context)
    : GenericRepository<TradeHistory>(context), ITradeHistoryRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<bool> IsTreeInPendingTradeAsync(int userTreeId)
    {
        return await _context.TradeHistory
            .AnyAsync(t => (t.TreeOwnerAid== userTreeId )
                        && t.Status == TradeStatus.Pending);
    }

}