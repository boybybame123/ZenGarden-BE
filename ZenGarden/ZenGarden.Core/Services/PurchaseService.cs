using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IBagItemRepository _bagItemRepo;
    private readonly IBagRepository _bagRepo;
    private readonly IItemDetailRepository _itemDetailRepo;
    private readonly IItemRepository _itemRepo;
    private readonly INotificationService _notificationService;
    private readonly IPurchaseHistoryRepository _purchaseHistoryRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWalletRepository _walletRepo;

    public PurchaseService(
        IWalletRepository walletRepo,
        IItemRepository itemRepo,
        IBagRepository bagRepo,
        IBagItemRepository bagItemRepo,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IItemDetailRepository itemDetailRepo,
        IPurchaseHistoryRepository purchaseHistoryRepo)
    {
        _walletRepo = walletRepo;
        _itemRepo = itemRepo;
        _bagRepo = bagRepo;
        _bagItemRepo = bagItemRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _itemDetailRepo = itemDetailRepo;
        _purchaseHistoryRepo = purchaseHistoryRepo;
    }

    public async Task<string> PurchaseItem(int userId, int itemId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Validate purchase conditions
            var (wallet, item) = await ValidatePurchaseConditions(userId, itemId);

            // 2. Process payment
            await ProcessPayment(wallet, item);

            // 3. Update inventory
            await UpdateInventory(userId, itemId, item);

            // 4. Record transaction
            await RecordPurchaseHistory(userId, itemId, item);

            // 5. Send notification
            await SendNotification(userId);

            await _unitOfWork.CommitTransactionAsync();
            return "Purchase successful.";
        }
        catch (PurchaseException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ex.Message;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new PurchaseException($"Purchase failed: {ex.Message}");
        }
    }

    private async Task<(Wallet wallet, Item item)> ValidatePurchaseConditions(int userId, int itemId)
    {
        var wallet = await _walletRepo.GetByUserIdAsync(userId)
                     ?? throw new PurchaseException("Wallet not found.");

        var item = await _itemRepo.GetByIdAsync(itemId)
                   ?? throw new PurchaseException("Item not found.");

        if (item.Status != ItemStatus.Active)
            throw new PurchaseException("Item is not available for purchase.");

        if (item.Cost == null || wallet.Balance < item.Cost.Value)
            throw new PurchaseException("Insufficient balance.");

        if (item.ItemDetail?.MonthlyPurchaseLimit is int limit && limit > 0)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var purchaseCount = await _purchaseHistoryRepo.CountPurchaseThisMonth(userId, itemId, startOfMonth);

            if (purchaseCount >= limit)
                throw new PurchaseException("Purchase limit for this month reached.");
        }

        if (item.ItemDetail?.IsUnique == true)
        {
            var bagItem = await _bagItemRepo.GetByBagAndItemAsync(userId, itemId);
            if (bagItem != null)
                throw new PurchaseException("Item is unique and can only be purchased once.");
        }

        return (wallet, item);
    }

    private async Task ProcessPayment(Wallet wallet, Item item)
    {
        wallet.Balance -= item.Cost!.Value;
        wallet.UpdatedAt = DateTime.UtcNow;
        _walletRepo.Update(wallet);
        await _unitOfWork.CommitAsync();
    }

    private async Task UpdateInventory(int userId, int itemId, Item item)
    {
        var bag = await _bagRepo.GetByUserIdAsync(userId)
                  ?? throw new PurchaseException("Bag not found.");

        var bagItem = await _bagItemRepo.GetByBagAndItemAsync(bag.BagId, itemId);
        if (bagItem != null)
        {
            bagItem.Quantity += 1;
            bagItem.UpdatedAt = DateTime.UtcNow;
            _bagItemRepo.Update(bagItem);
        }
        else
        {
            await _bagItemRepo.CreateAsync(new BagItem
            {
                BagId = bag.BagId,
                ItemId = itemId,
                Quantity = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (item.ItemDetail != null)
        {
            item.ItemDetail.Sold += 1;
            _itemDetailRepo.Update(item.ItemDetail);
        }
    }

    private async Task RecordPurchaseHistory(int userId, int itemId, Item item)
    {
        await _purchaseHistoryRepo.CreateAsync(new PurchaseHistory
        {
            UserId = userId,
            ItemId = itemId,
            TotalPrice = item.Cost!.Value,
            CreatedAt = DateTime.UtcNow,
            Status = PurchaseHistoryStatus.Approved
        });
    }

    private async Task SendNotification(int userId)
    {
        await _notificationService.PushNotificationAsync(userId, "Item", "Purchase successful.");
    }
}

public class PurchaseException : Exception
{
    public PurchaseException(string message) : base(message)
    {
    }
}