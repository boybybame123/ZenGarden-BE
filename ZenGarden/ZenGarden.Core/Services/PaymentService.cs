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

        // 2. Tạo PaymentIntent trước
        var paymentIntentService = new PaymentIntentService(_stripeClient);
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(package.Price * 100),
            Currency = "vnd",
            Metadata = new Dictionary<string, string>
        {
            { "user_id", request.UserId.ToString() },
            { "package_id", package.PackageId.ToString() }
        },
            PaymentMethodTypes = new List<string> { "card" }
        };

        var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

        // 3. Tạo Checkout Session với PaymentIntent đã tạo
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(package.Price * 100),
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
            SuccessUrl = $"https://localhost:7262/api/Payment/success?paymentIntentId={paymentIntent.Id}",
            CancelUrl = "https://zengarden-be.onrender.com/cancel",
            Metadata = new Dictionary<string, string>
        {
            { "user_id", request.UserId.ToString() },
            { "package_id", package.PackageId.ToString() }
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
            TransactionRef = paymentIntent.Id // Luôn sử dụng paymentIntent.Id
        };

        await _transactionRepository.CreateAsync(transaction);
        await _unitOfWork.CommitAsync();

        // 5. Return response
        return new CheckoutResponse
        {
            CheckoutUrl = session.Url,
            SessionId = session.Id,
            PaymentIntentId = paymentIntent.Id, // Đảm bảo luôn có giá trị
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


    public async Task<StripePaymentInfo> GetStripePaymentInfoAsync(string paymentIntentId)
    {
        try
        {
            // 1. Khởi tạo dịch vụ PaymentIntent của Stripe
            var paymentIntentService = new PaymentIntentService(_stripeClient);

            // 2. Gọi API Stripe để lấy thông tin PaymentIntent
            var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);

            // 3. Trả về thông tin chi tiết
            return new StripePaymentInfo
            {
                PaymentIntentId = paymentIntent.Id,
                Amount = paymentIntent.Amount / 100m, // Chuyển từ cents sang đơn vị tiền tệ
                Currency = paymentIntent.Currency,
                Status = paymentIntent.Status, // "succeeded", "processing", "requires_payment_method", etc.
                Created = paymentIntent.Created,
                PaymentMethodId = paymentIntent.PaymentMethodId,
                Metadata = paymentIntent.Metadata // Chứa user_id, package_id (nếu có)
            };
        }
        catch (StripeException ex)
        {
            // Xử lý lỗi từ Stripe (ví dụ: payment intent không tồn tại)
            throw new Exception($"Stripe error: {ex.Message}");
        }
    }
    public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            // 1. Kiểm tra trạng thái hiện tại của PaymentIntent
            var paymentIntentService = new PaymentIntentService(_stripeClient);
            var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);

            // 2. Chỉ hủy nếu ở trạng thái có thể hủy
            if (paymentIntent.Status == "requires_payment_method" ||
                paymentIntent.Status == "requires_confirmation")
            {
                // 3. Hủy PaymentIntent trên Stripe
                var canceledIntent = await paymentIntentService.CancelAsync(paymentIntentId);

                // 4. Cập nhật database
                var transaction = await _transactionRepository.FindByRefAsync(paymentIntentId);
                if (transaction != null)
                {
                    // Nên dùng TransactionStatus.Cancelled thay vì Failed
                    transaction.Status = TransactionStatus.Failed;
                    transaction.TransactionTime = DateTime.UtcNow;

                    _transactionRepository.Update(transaction);
                    await _unitOfWork.CommitAsync();
                }

                return true;
            }

            // 5. Xử lý các trường hợp không thể hủy
            if (paymentIntent.Status == "succeeded")
            {
                throw new InvalidOperationException("Không thể hủy giao dịch đã thành công. Hãy sử dụng chức năng hoàn tiền.");
            }

            return false;
        }
        catch (StripeException ex)
        {
            // Ghi log lỗi từ Stripe
            
            throw new Exception($"Stripe error: {ex.Message}");
        }
    }

}