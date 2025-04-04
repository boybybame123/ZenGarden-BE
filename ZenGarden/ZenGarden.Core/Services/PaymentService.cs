using Stripe;
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

        // 2. Create Stripe Checkout Session
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(package.Price * 100), // Amount in cents
                    Currency = "vnd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = package.Name,
                        Description = $"Nạp {package.Amount} Zen vào ví"
                    }
                },
                Quantity = 1
            }
        },
            Mode = "payment",
            SuccessUrl = "https://yourdomain.com/success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = "https://yourdomain.com/cancel",
            Metadata = new Dictionary<string, string>
        {
            { "user_id", request.UserId.ToString() },
            { "package_id", package.PackageId.ToString() }
        }
        };

        var service = new SessionService(_stripeClient);
        var session = await service.CreateAsync(options);

        // 3. Save transaction
        var transaction = new Transactions
        {
            UserId = request.UserId,
            WalletId = request.WalletId,
            PackageId = package.PackageId,
            Amount = package.Amount,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Pending,
            PaymentMethod = "Stripe",
            TransactionRef = session.PaymentIntentId // or session.Id for checkout session
        };

        await _transactionRepository.CreateAsync(transaction);
        await _unitOfWork.CommitAsync();

        // 4. Return checkout link
        return new CheckoutResponse
        {
            CheckoutUrl = session.Url, // Direct Stripe Checkout link
            SessionId = session.Id,
            PaymentIntentId = session.PaymentIntentId,
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
                _walletRepository.Update(wallet);
                await _unitOfWork.CommitAsync();
            }

            _transactionRepository.Update(transaction);
            await _unitOfWork.CommitAsync();
        }
    }
}