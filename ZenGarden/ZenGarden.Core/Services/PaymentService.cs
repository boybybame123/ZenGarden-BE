﻿using Stripe;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class PaymentService
{
    private readonly IPackageRepository _packageRepository;
    private readonly StripeClient _stripeClient;
    private readonly ITransactionsRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWalletRepository _walletRepository;

    public PaymentService(
        ITransactionsRepository transactionRepository,
        IWalletRepository walletRepository,
        IPackageRepository packageRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
        _stripeClient =
            new StripeClient(
                "sk_test_51QytHoLRxlQvzGwK9SWbMbZ0IdvtVY2I7564umiV1bSZBdYNyKsAxMGCtlysfAkStAemSjAtIQLpVCQXtC0qJrez00XPcVdOUq");
    }

    public async Task<string> CreatePaymentIntent(CreatePaymentRequest request)
    {
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null || !package.IsActive)
            throw new Exception("Gói nạp không hợp lệ.");

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(package.Price * 100),
            Currency = "vnd",
            PaymentMethodTypes = new List<string> { "card" }
        };

        var service = new PaymentIntentService(_stripeClient);
        var paymentIntent = await service.CreateAsync(options);

        var transaction = new Transactions
        {
            UserId = request.UserId,
            WalletId = request.WalletId,
            PackageId = request.PackageId,
            Amount = package.Amount,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Pending,
            PaymentMethod = "Stripe",
            TransactionRef = paymentIntent.Id
        };

        await _transactionRepository.CreateAsync(transaction);
        return paymentIntent.ClientSecret;
    }

    public async Task HandlePaymentSucceeded(string paymentIntentId)
    {
        var transaction = await _transactionRepository.FindByRefAsync(paymentIntentId);
        if (transaction != null && transaction.Status == TransactionStatus.Pending)
        {
            transaction.Status = TransactionStatus.Completed;
            transaction.TransactionTime = DateTime.UtcNow;

            var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
            if (wallet != null)
            {
                wallet.Balance += transaction.Amount ?? 0;
                wallet.UpdatedAt = DateTime.UtcNow;
                _walletRepository.Update(wallet);
                await _unitOfWork.CommitAsync();
            }

            _transactionRepository.Update(transaction);
            await _unitOfWork.CommitAsync();
        }
    }
}