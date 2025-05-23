using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TransactionsRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<Transactions>(context, redisService), ITransactionsRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Transactions?> FindByRefAsync(string transactionRef)
    {
        return await _context.Set<Transactions>()
            .FirstOrDefaultAsync(t => t.TransactionRef == transactionRef);
    }

    public async Task<List<Transactions>?> ListAllTransactionsByIdAsync(int userId)
    {
        return await _context.Set<Transactions>()
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Transactions>?> ListPendingTransactionsAsyn()
    {
        var fifteenMinutesAgo = DateTime.Now.AddMinutes(-15);
        return await _context.Set<Transactions>()
            .Where(t => t.CreatedAt < fifteenMinutesAgo && t.Status == TransactionStatus.Pending)
            .ToListAsync();
    }
}