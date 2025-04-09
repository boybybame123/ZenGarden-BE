using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TradeHistoryRepository : GenericRepository<TradeHistory>, ITradeHistoryRepository
{
    private readonly ZenGardenContext _context;

    public TradeHistoryRepository(ZenGardenContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> IsTreeInPendingTradeAsync(int userTreeId)
    {
        return await _context.TradeHistory
            .AnyAsync(t => (t.TreeOwnerAid == userTreeId)
                        && t.Status == TradeStatus.Pending);
    }
    public async Task<List<TradeHistory>> GetAllTradeHistoriesbyStatutsAsync(int Statuts)
    {
        return await _context.TradeHistory
    .Where(th => th.Status == (TradeStatus)Statuts)
    .ToListAsync();
    }
}
