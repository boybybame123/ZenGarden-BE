using Stripe;
using Stripe.Checkout;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class PaymentService(
    ITransactionsRepository transactionRepository,
    IWalletRepository walletRepository,
    INotificationService notificationService,
    IPackageRepository packageRepository,
    IRedisService redisService,
    IUnitOfWork unitOfWork)
{
    private readonly StripeClient _stripeClient = new(
        "sk_test_51QytHoLRxlQvzGwK9SWbMbZ0IdvtVY2I7564umiV1bSZBdYNyKsAxMGCtlysfAkStAemSjAtIQLpVCQXtC0qJrez00XPcVdOUq");

    public async Task<CheckoutResponse> CreatePayment(CreatePaymentRequest request)
    {
        // 1. Validate package
        var package = await packageRepository.GetByIdAsync(request.PackageId);
        if (package is not { IsActive: true })
            throw new Exception("Invalid package");
        var amountInCents = (long)(package.Price * 100);
        // 2. Create PaymentIntent first
        var paymentIntentService = new PaymentIntentService(_stripeClient);
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = "usd",
            Metadata = new Dictionary<string, string>
            {
                { "user_id", request.UserId.ToString() },
                { "package_id", package.PackageId.ToString() }
            },
            PaymentMethodTypes = ["card"],
            Description = $"Zen purchase - {package.Name}" // English-only description
        };

        var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

        // 3. Create Checkout Session with English-only text
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = amountInCents,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = package.Name,
                            Description = $"Credit {(int)Math.Floor(package.Amount)} Zen to wallet" // English
                            // Optional logo
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            Locale = "en",
            SuccessUrl = $"https://zengarden-be-fdre.onrender.com/api/Payment/success/{paymentIntent.Id}",
            CancelUrl = $"https://zengarden-be-fdre.onrender.com/api/Payment/cancel/{paymentIntent.Id}",
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
            }
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

        await transactionRepository.CreateAsync(transaction);
        await unitOfWork.CommitAsync();
   
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
        var transaction = await transactionRepository.FindByRefAsync(paymentIntentId);
        if (transaction is null)
            throw new Exception("Transaction not found");
        if (transaction.UserId == null)
            throw new Exception("User not found");

        // Check if the transaction is pending
        if (transaction is { Status: TransactionStatus.Pending })
        {
            transaction.Status = TransactionStatus.Completed;
            transaction.TransactionTime = DateTime.UtcNow;

            var wallet = await walletRepository.GetByIdAsync(transaction.WalletId);
            if (wallet != null)
            {
                wallet.Balance += transaction.Amount ?? 0;
                wallet.UpdatedAt = DateTime.UtcNow;
                wallet.LastTransactionAt = DateTime.UtcNow;
                walletRepository.Update(wallet);
                await unitOfWork.CommitAsync();
            }

            transactionRepository.Update(transaction);
            await unitOfWork.CommitAsync();
        }
        else
        {
            throw new Exception("Transaction not found or already completed");
        }

        await notificationService.PushNotificationAsync(transaction.UserId.Value, "Payment", "Success");
    }

    public async Task HandlePaymentCanceled(string paymentIntentId)
    {
        var transaction = await transactionRepository.FindByRefAsync(paymentIntentId);
        if (transaction is null)
            throw new Exception("Transaction not found");
        if (transaction.UserId == null)
            throw new Exception("User not found");

        if (transaction is { Status: TransactionStatus.Pending })
        {
            transaction.Status = TransactionStatus.Failed;
            transactionRepository.Update(transaction);
            await unitOfWork.CommitAsync();
            await notificationService.PushNotificationAsync(transaction.UserId.Value, "Payment", "Canceled");
        }
        else
        {
            throw new Exception("Transaction not found or already completed");
        }
    }
}