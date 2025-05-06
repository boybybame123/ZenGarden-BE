using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class WalletRepository(ZenGardenContext context) : GenericRepository<Wallet>(context), IWalletRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Wallet?> GetByUserIdAsync(int userId)
    {
        return await _context.Wallet.FirstOrDefaultAsync(x => x.UserId == userId);
    }
    public async Task<decimal> GetTotalBalanceAsync()
    {
        return await _context.Wallet.SumAsync(x => x.Balance ?? 0);
    }
}