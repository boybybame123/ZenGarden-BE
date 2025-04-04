using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;


namespace ZenGarden.Core.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(
        IWalletRepository walletRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> GetBalanceAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            throw new Exception($"Wallet for user {userId} not found");
        }

        return (decimal)wallet.Balance;
    }

    public async Task<Wallet> GetWalletAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            throw new Exception($"Wallet for user {userId} not found");
        }

        return _mapper.Map<Wallet>(wallet);
    }





    public async Task LockWalletAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            throw new Exception($"Wallet for user {userId} not found");
        }

        wallet.IsLocked = true;
        wallet.UpdatedAt = DateTime.UtcNow;

        _walletRepository.Update(wallet); // Changed to synchronous Update
        await _unitOfWork.CommitAsync();
    }

    public async Task UnlockWalletAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            throw new Exception($"Wallet for user {userId} not found");
        }

        wallet.IsLocked = false;
        wallet.UpdatedAt = DateTime.UtcNow;

        _walletRepository.Update(wallet); // Changed to synchronous Update
        await _unitOfWork.CommitAsync();
    }

}