﻿using Stripe;
using Stripe.Checkout;
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

    public async Task<CheckoutResponse> CreatePayment(CreatePaymentRequest request)
    {
        // 1. Validate package
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null || !package.IsActive)
            throw new Exception("Invalid package");

        // 2. Create PaymentIntent first
        var paymentIntentService = new PaymentIntentService(_stripeClient);
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(package.Price),
            Currency = "usd",
            Metadata = new Dictionary<string, string>
        {
            { "user_id", request.UserId.ToString() },
            { "package_id", package.PackageId.ToString() }
        },
            PaymentMethodTypes = new List<string> { "card" },
            Description = $"Zen purchase - {package.Name}" // English-only description
        };

        var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

        // 3. Create Checkout Session with English-only text
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            PaymentMethodOptions = new SessionPaymentMethodOptionsOptions
            {
                Link = new SessionPaymentMethodOptionsLinkOptions
                {
                    Enabled = false
                }
            },
            LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(package.Price),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = package.Name,
                        Description = $"Credit {(int)Math.Floor(package.Amount)} Zen to wallet", // English
                        // Optional logo
                    }
                },
                Quantity = 1
            }
        },
            Mode = "payment",
            Locale ="en",
            SuccessUrl = $"https://zengarden-be.onrender.com/api/Payment/success?paymentIntentId={paymentIntent.Id}",
            CancelUrl = $"https://zengarden-be.onrender.com/api/Payment/cancel?paymentIntentId={paymentIntent.Id}",
            Metadata = new Dictionary<string, string>
        {
            { "user_id", request.UserId.ToString() },
            { "package_id", package.PackageId.ToString() }
        },
            CustomText = new SessionCustomTextOptions
            {
                Submit = new SessionCustomTextSubmitOptions
                {
                    Message = "Complete Payment" // English button text
                }
            },

        };

        var service = new SessionService(_stripeClient);
        var session = await service.CreateAsync(options);

        // 4. Save transaction
        var transaction = new Transactions
        {
            UserId = request.UserId,
            WalletId = request.WalletId,
            PackageId = package.PackageId,
            Amount = package.Amount,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Pending,
            PaymentMethod = "Stripe",
            TransactionRef = paymentIntent.Id
        };

        await _transactionRepository.CreateAsync(transaction);
        await _unitOfWork.CommitAsync();

        // 5. Return response
        return new CheckoutResponse
        {
            CheckoutUrl = session.Url,
            SessionId = session.Id,
            PaymentIntentId = paymentIntent.Id,
            Amount = package.Price,
            PackageName = package.Name
        };
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
                wallet.LastTransactionAt = DateTime.UtcNow;
                _walletRepository.Update(wallet);
                await _unitOfWork.CommitAsync();
            }

            _transactionRepository.Update(transaction);
            await _unitOfWork.CommitAsync();
        }
    }
    public async Task HandlePaymentCanceled(string paymentIntentId)
    {
        var transaction = await _transactionRepository.FindByRefAsync(paymentIntentId);
        if (transaction != null && transaction.Status == TransactionStatus.Pending)
        {
            transaction.Status = TransactionStatus.Failed;
            _transactionRepository.Update(transaction);
            await _unitOfWork.CommitAsync();
        }
        
    }



}
