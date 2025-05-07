using Stripe;
using Stripe.Checkout;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ZenGarden.Core.Services;

public class PaymentService
{
    private readonly ITransactionsRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly INotificationService _notificationService;
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;
    private readonly StripeClient _stripeClient;
    private readonly string _successUrl;
    private readonly string _cancelUrl;
    private readonly int _adminWalletId;

    public PaymentService(
        ITransactionsRepository transactionRepository,
        IWalletRepository walletRepository,
        INotificationService notificationService,
        IPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentService> logger,
        IConfiguration configuration)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _notificationService = notificationService;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;

        var stripeSecretKey = configuration["Stripe:SecretKey"] 
            ?? throw new ArgumentNullException("Stripe:SecretKey configuration is missing");
        _stripeClient = new StripeClient(stripeSecretKey);
        
        _successUrl = configuration["Stripe:SuccessUrl"] 
            ?? throw new ArgumentNullException("Stripe:SuccessUrl configuration is missing");
        _cancelUrl = configuration["Stripe:CancelUrl"] 
            ?? throw new ArgumentNullException("Stripe:CancelUrl configuration is missing");
        _adminWalletId = configuration.GetValue<int>("AdminWalletId", 18);
    }

    public async Task<CheckoutResponse> CreatePayment(CreatePaymentRequest request)
    {
        try
        {
            _logger.LogInformation($"Creating payment for user {request.UserId}, package {request.PackageId}");

            // 1. Validate package
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
            {
                _logger.LogWarning($"Package {request.PackageId} not found");
                throw new KeyNotFoundException("Package not found");
            }

            if (!package.IsActive)
            {
                _logger.LogWarning($"Package {request.PackageId} is not active");
                throw new InvalidOperationException("Package is not active");
            }

            var amountInCents = (long)(package.Price * 100);

            // 2. Create PaymentIntent
            var paymentIntent = await CreatePaymentIntent(request.UserId, package, amountInCents);

            // 3. Create Checkout Session
            var session = await CreateCheckoutSession(request.UserId, package, amountInCents, paymentIntent.Id);

            // 4. Save transaction
            var transaction = await SaveTransaction(request, package, paymentIntent.Id);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating payment for user {request.UserId}, package {request.PackageId}");
            throw;
        }
    }

    private async Task<PaymentIntent> CreatePaymentIntent(int userId, Packages package, long amountInCents)
    {
        var paymentIntentService = new PaymentIntentService(_stripeClient);
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = "usd",
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId.ToString() },
                { "package_id", package.PackageId.ToString() }
            },
            PaymentMethodTypes = ["card"],
            Description = $"Zen purchase - {package.Name}"
        };

        return await paymentIntentService.CreateAsync(paymentIntentOptions);
    }

    private async Task<Session> CreateCheckoutSession(int userId, Packages package, long amountInCents, string paymentIntentId)
    {
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
                            Description = $"Credit {(int)Math.Floor(package.Amount)} Zen to wallet"
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            Locale = "en",
            SuccessUrl = $"{_successUrl}?paymentIntentId={paymentIntentId}",
            CancelUrl = $"{_cancelUrl}?paymentIntentId={paymentIntentId}",
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId.ToString() },
                { "package_id", package.PackageId.ToString() }
            },
            CustomText = new SessionCustomTextOptions
            {
                Submit = new SessionCustomTextSubmitOptions
                {
                    Message = "Complete Payment"
                }
            }
        };

        var service = new SessionService(_stripeClient);
        return await service.CreateAsync(options);
    }

    private async Task<Transactions> SaveTransaction(CreatePaymentRequest request, Packages package, string paymentIntentId)
    {
        var transaction = new Transactions
        {
            UserId = request.UserId,
            WalletId = request.WalletId,
            PackageId = package.PackageId,
            Amount = package.Amount,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Pending,
            PaymentMethod = "Stripe",
            TransactionRef = paymentIntentId
        };

        await _transactionRepository.CreateAsync(transaction);
        await _unitOfWork.CommitAsync();
        return transaction;
    }

    public async Task HandlePaymentSucceeded(string paymentIntentId)
    {
        try
        {
            _logger.LogInformation($"Handling successful payment for payment intent {paymentIntentId}");

            var transaction = await _transactionRepository.FindByRefAsync(paymentIntentId);
            if (transaction == null)
            {
                _logger.LogWarning($"Transaction not found for payment intent {paymentIntentId}");
                throw new KeyNotFoundException("Transaction not found");
            }

            if (transaction.UserId == null)
            {
                _logger.LogWarning($"User not found for transaction {transaction.TransactionId}");
                throw new KeyNotFoundException("User not found");
            }

            if (transaction.Status != TransactionStatus.Pending)
            {
                _logger.LogWarning($"Transaction {transaction.TransactionId} is not pending");
                throw new InvalidOperationException("Transaction is not pending");
            }

            await ProcessSuccessfulPayment(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling successful payment for payment intent {paymentIntentId}");
            throw;
        }
    }

    private async Task ProcessSuccessfulPayment(Transactions transaction)
    {
        transaction.Status = TransactionStatus.Completed;
        transaction.TransactionTime = DateTime.UtcNow;

        var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
        var adminWallet = await _walletRepository.GetByIdAsync(_adminWalletId);

        if (wallet == null || adminWallet == null)
        {
            _logger.LogError($"Wallet not found for transaction {transaction.TransactionId}");
            throw new KeyNotFoundException("Wallet not found");
        }

        // Update wallet balances
        await UpdateWalletBalances(wallet, adminWallet, transaction.Amount ?? 0);

        _transactionRepository.Update(transaction);
        await _unitOfWork.CommitAsync();

        if (transaction.UserId.HasValue)
        {
            await _notificationService.PushNotificationAsync(
                transaction.UserId.Value,
                "Payment Successful",
                "Your payment has been processed successfully"
            );
        }
        else
        {
            _logger.LogWarning($"No UserId found for transaction {transaction.TransactionId}");
        }

        _logger.LogInformation($"Successfully processed payment for transaction {transaction.TransactionId}");
    }

    private async Task UpdateWalletBalances(Wallet userWallet, Wallet adminWallet, decimal amount)
    {
        userWallet.Balance += amount;
        userWallet.UpdatedAt = DateTime.UtcNow;
        userWallet.LastTransactionAt = DateTime.UtcNow;

        adminWallet.Balance += amount;
        adminWallet.UpdatedAt = DateTime.UtcNow;
        adminWallet.LastTransactionAt = DateTime.UtcNow;

        _walletRepository.Update(userWallet);
        _walletRepository.Update(adminWallet);
        await _unitOfWork.CommitAsync();
    }

    public async Task HandlePaymentCanceled(string paymentIntentId)
    {
        try
        {
            _logger.LogInformation($"Handling canceled payment for payment intent {paymentIntentId}");

            var transaction = await _transactionRepository.FindByRefAsync(paymentIntentId);
            if (transaction == null)
            {
                _logger.LogWarning($"Transaction not found for payment intent {paymentIntentId}");
                throw new KeyNotFoundException("Transaction not found");
            }

            if (transaction.UserId == null)
            {
                _logger.LogWarning($"User not found for transaction {transaction.TransactionId}");
                throw new KeyNotFoundException("User not found");
            }

            if (transaction.Status != TransactionStatus.Pending)
            {
                _logger.LogWarning($"Transaction {transaction.TransactionId} is not pending");
                throw new InvalidOperationException("Transaction is not pending");
            }

            transaction.Status = TransactionStatus.Failed;
            _transactionRepository.Update(transaction);
            await _unitOfWork.CommitAsync();

            if (transaction.UserId.HasValue)
            {
                await _notificationService.PushNotificationAsync(
                    transaction.UserId.Value,
                    "Payment Canceled",
                    "Your payment has been canceled"
                );
            }
            else
            {
                _logger.LogWarning($"No UserId found for transaction {transaction.TransactionId}");
            }

            _logger.LogInformation($"Successfully processed payment cancellation for transaction {transaction.TransactionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling canceled payment for payment intent {paymentIntentId}");
            throw;
        }
    }
}