using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ZenGarden.Core.Services;

public class UseItemService(
    IUserConfigRepository userConfigRepository,
    IBagItemRepository bagItemRepository,
    IItemRepository itemRepository,
    IItemDetailRepository itemDetailRepository,
    IBagRepository bagRepository,
    INotificationService notificationService,
    IUnitOfWork unitOfWork,
    IRedisService redisService,
    ILogger<UseItemService> logger
) : IUseItemService
{
    public async Task<string> UseItemAsync(int userId, int itembagId)
    {
        try
        {
            logger.LogInformation($"Attempting to use item {itembagId} for user {userId}");

            var item = await bagItemRepository.GetByIdAsync(itembagId);
            if (item == null)
            {
                logger.LogWarning($"Item {itembagId} not found");
                throw new KeyNotFoundException("Item not found");
            }

            if (item.Quantity <= 0)
            {
                logger.LogWarning($"Item {itembagId} has zero quantity");
                throw new InvalidOperationException("Item quantity is zero");
            }

            var bag = await bagRepository.GetByIdAsync(item.BagId);
            if (bag == null)
            {
                logger.LogWarning($"Bag not found for item {itembagId}");
                throw new KeyNotFoundException("Bag not found");
            }

            if (bag.UserId != userId)
            {
                logger.LogWarning($"User {userId} not authorized to use item {itembagId}");
                throw new UnauthorizedAccessException("User not authorized to use this item");
            }

            if (item.ItemId == null)
            {
                logger.LogWarning($"Item {itembagId} has no associated ItemId");
                throw new InvalidOperationException("Item has no associated ItemId");
            }

            var itemDetail = await itemDetailRepository.GetItemDetailsByItemId(item.ItemId.Value);
            if (itemDetail == null)
            {
                logger.LogWarning($"Item detail not found for item {item.ItemId}");
                throw new KeyNotFoundException("Item detail not found");
            }

            if (item.Item == null)
            {
                logger.LogWarning($"Item details not found for item {item.ItemId}");
                throw new KeyNotFoundException("Item details not found");
            }

            var userConfig = await userConfigRepository.GetByIdAsync(userId);
            if (userConfig == null)
            {
                logger.LogWarning($"User config not found for user {userId}");
                throw new KeyNotFoundException("User config not found");
            }

            // Unequip any existing item of the same type
            await bagItemRepository.UnequipByBagIdAndItemTypeAsync(bag.BagId, item.Item.Type);

            // Apply item to UserConfig
            string result = ApplyItemToUserConfig(item.Item.Type, itemDetail.MediaUrl, userConfig);
            if (result != "success")
            {
                logger.LogWarning($"Failed to apply item {itembagId} to user config: {result}");
                return result;
            }

            item.isEquipped = true;
            userConfig.UpdatedAt = DateTime.UtcNow;

            userConfigRepository.Update(userConfig);
            bagItemRepository.Update(item);
            await unitOfWork.CommitAsync();

            var cacheKey = $"BagItems_{item.BagId}";
            await redisService.DeleteKeyAsync(cacheKey);

            await notificationService.PushNotificationAsync(userId, "Use Item",
                $"You have successfully used {item.Item.Name} item");

            logger.LogInformation($"Successfully used item {itembagId} for user {userId}");
            return "Use Item success";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error using item {itembagId} for user {userId}");
            throw;
        }
    }

    private string ApplyItemToUserConfig(ItemType itemType, string mediaUrl, UserConfig userConfig)
    {
        switch (itemType)
        {
            case ItemType.Background:
                userConfig.BackgroundConfig = mediaUrl;
                break;
            case ItemType.Music:
                userConfig.SoundConfig = mediaUrl;
                break;
            case ItemType.Avatar:
                userConfig.ImageUrl = mediaUrl;
                break;
            case ItemType.XpBoostTree:
            case ItemType.XpProtect:
                // These items are handled separately
                break;
            default:
                return "Item type not supported";
        }
        return "success";
    }

    public async Task UseItemXpBoostTree(int userId)
    {
        try
        {
            logger.LogInformation($"Attempting to use XP Boost Tree for user {userId}");

            var itemBag = await bagRepository.GetEquippedItemAsync(userId, ItemType.XpBoostTree);
            if (itemBag == null)
            {
                logger.LogWarning($"No XP Boost Tree found for user {userId}");
                return;
            }

            if (itemBag.Quantity <= 0)
            {
                logger.LogWarning($"XP Boost Tree quantity is zero for user {userId}");
                return;
            }

            itemBag.Quantity--;
            itemBag.UpdatedAt = DateTime.UtcNow;
            bagItemRepository.Update(itemBag);
            await unitOfWork.CommitAsync();

            var cacheKey = $"BagItems_{itemBag.BagId}";
            await redisService.DeleteKeyAsync(cacheKey);

            logger.LogInformation($"Successfully used XP Boost Tree for user {userId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error using XP Boost Tree for user {userId}");
            throw;
        }
    }

    public async Task Cancel(int bagItemId)
    {
        var item = await bagItemRepository.GetByIdAsync(bagItemId);
        if (item == null)
            throw new KeyNotFoundException("Item not found");
        item.isEquipped = false;
        item.UpdatedAt = DateTime.UtcNow;
        bagItemRepository.Update(item);
        await unitOfWork.CommitAsync();
        var cacheKey = $"BagItems_{item.BagId}";
        await redisService.DeleteKeyAsync(cacheKey);
    }

    public async Task<string> GiftRandomItemFromListAsync(int userId)
    {
        try
        {
            logger.LogInformation($"Attempting to gift random item to user {userId}");

            // Get the list of giftable items  
            var giftableItems = await itemRepository.GetListItemGift();
            if (giftableItems == null || !giftableItems.Any())
            {
                logger.LogWarning("No giftable items available");
                throw new InvalidOperationException("No giftable items available");
            }

            // Randomly select an item from the list  
            var random = new Random();
            var randomItem = giftableItems[random.Next(giftableItems.Count)];
            logger.LogInformation($"Selected random item {randomItem.ItemId} ({randomItem.Name}) for user {userId}");

            // Check if the user has a bag  
            var bag = await bagRepository.GetByUserIdAsync(userId);
            if (bag == null)
            {
                logger.LogWarning($"User bag not found for user {userId}");
                throw new KeyNotFoundException("User bag not found");
            }

            // Check if the item already exists in the bag  
            var existingBagItem = await bagItemRepository.GetByBagAndItemAsync(bag.BagId, randomItem.ItemId);
            if (existingBagItem != null)
            {
                // Increment the quantity if the item already exists  
                existingBagItem.Quantity++;
                existingBagItem.UpdatedAt = DateTime.UtcNow;
                bagItemRepository.Update(existingBagItem);
                logger.LogInformation($"Incremented quantity of existing item {randomItem.ItemId} for user {userId}");
            }
            else
            {
                // Create a new BagItem for the selected item  
                var newBagItem = new BagItem
                {
                    BagId = bag.BagId,
                    ItemId = randomItem.ItemId,
                    Quantity = 1,
                    isEquipped = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add the item to the user's bag  
                await bagItemRepository.CreateAsync(newBagItem);
                logger.LogInformation($"Created new bag item {randomItem.ItemId} for user {userId}");
            }

            await unitOfWork.CommitAsync();

            // Invalidate the cache  
            var cacheKey = $"BagItems_{bag.BagId}";
            await redisService.DeleteKeyAsync(cacheKey);

            // Notify the user  
            await notificationService.PushNotificationAsync(userId, "Gift Item",
                $"You have received a random item: {randomItem.Name}");

            logger.LogInformation($"Successfully gifted item {randomItem.ItemId} to user {userId}");
            return $"Gifted {randomItem.Name} to user {userId}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error gifting random item to user {userId}");
            throw;
        }
    }

    public async Task UseItemXpProtect(int userId)
    {
        try
        {
            logger.LogInformation($"Attempting to use XP Protect for user {userId}");

            var itemBagId = await bagRepository.GetItemByHavingUse(userId, ItemType.XpProtect);
            if (itemBagId <= 0)
            {
                logger.LogWarning($"No XP Protect item found for user {userId}");
                throw new KeyNotFoundException("XP Protect item not found");
            }

            var itemBag = await bagItemRepository.GetByIdAsync(itemBagId);
            if (itemBag == null)
            {
                logger.LogWarning($"XP Protect item {itemBagId} not found");
                throw new KeyNotFoundException("XP Protect item not found");
            }

            if (itemBag.Quantity <= 0)
            {
                logger.LogWarning($"XP Protect item quantity is zero for user {userId}");
                throw new InvalidOperationException("XP Protect item quantity is zero");
            }

            itemBag.Quantity--;
            itemBag.UpdatedAt = DateTime.UtcNow;
            bagItemRepository.Update(itemBag);
            await unitOfWork.CommitAsync();

            var cacheKey = $"BagItems_{itemBag.BagId}";
            await redisService.DeleteKeyAsync(cacheKey);

            logger.LogInformation($"Successfully used XP Protect for user {userId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error using XP Protect for user {userId}");
            throw;
        }
    }
}