using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IWalletService
{
    Task<decimal> GetBalanceAsync(int userId);
    Task<Wallet> GetWalletAsync(int userId);
    Task LockWalletAsync(int userId);
    Task UnlockWalletAsync(int userId);
}