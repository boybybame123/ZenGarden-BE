using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class WalletService(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : IWalletService
{
    public async Task<decimal> GetBalanceAsync(int userId)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId);
        if (wallet == null) throw new Exception($"Wallet for user {userId} not found");

        return wallet.Balance ?? 0m;
    }

    public async Task<Wallet> GetWalletAsync(int userId)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId);
        if (wallet == null) throw new Exception($"Wallet for user {userId} not found");

        return wallet;
    }

    public async Task<List<Wallet>> GetAllWalletAsync()
    {
        var wallets = await walletRepository.GetAllAsync();
        if (wallets == null) throw new Exception($"Wallets not found");
        return wallets;
    }


    public async Task LockWalletAsync(int userId)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId);
        if (wallet == null) throw new Exception($"Wallet for user {userId} not found");

        wallet.IsLocked = true;
        wallet.UpdatedAt = DateTime.UtcNow;

        walletRepository.Update(wallet); // Changed to synchronous Update
        await unitOfWork.CommitAsync();
    }

    public async Task UnlockWalletAsync(int userId)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId);
        if (wallet == null) throw new Exception($"Wallet for user {userId} not found");

        wallet.IsLocked = false;
        wallet.UpdatedAt = DateTime.UtcNow;

        walletRepository.Update(wallet); // Changed to synchronous Update
        await unitOfWork.CommitAsync();
    }

}