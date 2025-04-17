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
            .AnyAsync(t => t.TreeOwnerAid == userTreeId
                           && t.Status == TradeStatus.Pending);
    }

    public async Task<List<TradeHistory>> GetAllTradeHistoriesbyStatutsAsync(int status)
    {
        return await _context.TradeHistory
            .Where(th => th.Status == (TradeStatus)status)
            .ToListAsync();
    }

    public async Task<List<TradeHistory>> GetAllTradeHistoriesByOwneridAsync(int ownerId)
    {
        return await _context.TradeHistory
            .Where(th => th.TreeOwnerAid == ownerId)
            .ToListAsync();
    }

    public async Task<List<TradeHistory>> GetAllTradeHistoriesByNotOwneridAsync(int userId)
    {
        return await _context.TradeHistory
            .Where(th => th.TreeOwnerAid != userId)
            .ToListAsync();
    }
}