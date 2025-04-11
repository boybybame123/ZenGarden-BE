using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TransactionsRepository : GenericRepository<Transactions>, ITransactionsRepository
{
    private readonly ZenGardenContext _context;

    public TransactionsRepository(ZenGardenContext context) : base(context)
    {
        _context = context;
    }

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
}