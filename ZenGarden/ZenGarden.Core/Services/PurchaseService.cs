﻿using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class PurchaseService(
    IWalletRepository walletRepo,
    IItemRepository itemRepo,
    IBagRepository bagRepo,
    IBagItemRepository bagItemRepo,
    INotificationService notificationService,
    IUnitOfWork unitOfWork,
    IItemDetailRepository itemDetailRepo,
    IPurchaseHistoryRepository purchaseHistoryRepo,
    IRedisService redisService
)
    : IPurchaseService
{
    public async Task<string> PurchaseItem(int userId, int itemId)
    {
     

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
            await SendNotification(userId, item);

            await unitOfWork.CommitTransactionAsync();
            return "Purchase successful.";
        }
        catch (PurchaseException ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return ex.Message;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw new PurchaseException($"Purchase failed: {ex.Message}");
        }
    }

    private async Task<(Wallet wallet, Item item)> ValidatePurchaseConditions(int userId, int itemId)
    {
        var wallet = await walletRepo.GetByUserIdAsync(userId)
                     ?? throw new PurchaseException("Userid not found.");

        var item = await itemRepo.GetByIdAsync(itemId)
                   ?? throw new PurchaseException("Item not found.");
        var itemDetail = await itemDetailRepo.GetItemDetailsByItemId(itemId)
                         ?? throw new PurchaseException("Item detail not found.");
        if (item.Status != ItemStatus.Active)
            throw new PurchaseException("Item is not available for purchase.");

        if (item.Cost == null || wallet.Balance < item.Cost.Value)
            throw new PurchaseException("Insufficient balance.");

        if (itemDetail.MonthlyPurchaseLimit is { } limit and > 0)
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var purchaseCount = await purchaseHistoryRepo.CountPurchaseThisMonth(userId, itemId, startOfMonth);

            if (purchaseCount >= limit)
                throw new PurchaseException("You have reached the monthly purchase limit for this item.");
        }

        if (itemDetail.IsUnique != true) return (wallet, item);
        var bagItem = await bagItemRepo.GetByBagAndItemAsync(userId, itemId);
        if (bagItem != null)
            throw new PurchaseException("Item is unique and can only be purchased once.");

        return (wallet, item);
    }

    private async Task ProcessPayment(Wallet wallet, Item item)
    {
        if (item.Cost != null) wallet.Balance -= item.Cost.Value;
        wallet.UpdatedAt = DateTime.UtcNow;
        walletRepo.Update(wallet);
        await unitOfWork.CommitAsync();
    }

    private async Task UpdateInventory(int userId, int itemId, Item item)
    {
        var bag = await bagRepo.GetByUserIdAsync(userId)
                  ?? throw new PurchaseException("Bag not found.");

        var bagItem = await bagItemRepo.GetByBagAndItemAsync(bag.BagId, itemId);
        if (bagItem != null)
        {
            bagItem.Quantity += 1;
            bagItem.UpdatedAt = DateTime.UtcNow;
            bagItemRepo.Update(bagItem);
            await unitOfWork.CommitAsync();
            var cacheKey = $"BagItems_{bagItem.BagId}";
            await redisService.RemoveAsync(cacheKey);
        }
        else
        {
            await bagItemRepo.CreateAsync(new BagItem
            {
                BagId = bag.BagId,
                ItemId = itemId,
                Quantity = 1,
                isEquipped = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            var cacheKey = $"BagItems_{bag.BagId}";
            await redisService.RemoveAsync(cacheKey);
        }

        if (item.ItemDetail != null)
        {
            item.ItemDetail.Sold += 1;
            itemDetailRepo.Update(item.ItemDetail);
            await unitOfWork.CommitAsync();
        }
    }

    private async Task RecordPurchaseHistory(int userId, int itemId, Item item)
    {
        if (item.Cost != null)
            await purchaseHistoryRepo.CreateAsync(new PurchaseHistory
            {
                UserId = userId,
                ItemId = itemId,
                TotalPrice = item.Cost.Value,
                CreatedAt = DateTime.UtcNow,
                Status = PurchaseHistoryStatus.Approved
            });
            await unitOfWork.CommitAsync();
    }

    private async Task SendNotification(int userId, Item item)
    {
        await notificationService.PushNotificationAsync(userId, "Marketplace", $"Purchase successful item {item.Name}");
    }
}

public class PurchaseException(string message) : Exception(message);