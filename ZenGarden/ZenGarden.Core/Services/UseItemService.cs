using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UseItemService(
    IUserConfigRepository userConfigRepository,
    IBagItemRepository bagItemRepository,
    IItemRepository itemRepository,
    IItemDetailRepository itemDetailRepository,
    IBagRepository bagRepository,
    INotificationService notificationService,
    IUnitOfWork unitOfWork,
    IRedisService redisService
) : IUseItemService
{
    public async Task<string> UseItemAsync(int userId, int itembagId)
    {
        var item = await bagItemRepository.GetByIdAsync(itembagId);
        if (item == null || item.Quantity <= 0)
            throw new KeyNotFoundException("Item not found or quantity is zero");
        var bag = await bagRepository.GetByIdAsync(item.BagId);
        if (bag == null)
            throw new KeyNotFoundException("Bag not found");
        if (bag.UserId != userId)
            throw new UnauthorizedAccessException("User not authorized to use this item");

        if (item.ItemId != null)
        {
            var itemDetail = await itemDetailRepository.GetItemDetailsByItemId(item.ItemId.Value);
            if (itemDetail == null)
                throw new KeyNotFoundException("Item detail not found");

            if (item.Item == null)


                throw new KeyNotFoundException("Item details not found");

            var userConfig = await userConfigRepository.GetByIdAsync(userId);
            if (userConfig == null)
                throw new KeyNotFoundException("User config not found");


            await bagItemRepository.UnequipByBagIdAndItemTypeAsync(bag.BagId, item.Item.Type);


            // ✅ Apply item vào UserConfig
            switch (item.Item.Type)
            {
                case ItemType.Background:
                    userConfig.BackgroundConfig = itemDetail.MediaUrl;

                    break;
                case ItemType.Music:
                    userConfig.SoundConfig = itemDetail.MediaUrl;

                    break;
                case ItemType.Avatar:
                    userConfig.ImageUrl = itemDetail.MediaUrl;

                    break;
                case ItemType.XpBoostTree:

                    break;
                case ItemType.XpProtect:

                    break;
                default:
                    return "item not rule";
            }


            item.isEquipped = true;
            userConfig.UpdatedAt = DateTime.UtcNow;

            userConfigRepository.Update(userConfig);
        }

        bagItemRepository.Update(item);
        await unitOfWork.CommitAsync();
        var cacheKey = $"BagItems_{item.BagId}";
        await redisService.DeleteKeyAsync(cacheKey);

        await notificationService.PushNotificationAsync(userId, "Use Item",
            $"You have successfully used {item.Item.Name} item");


        return "Use Item success";
    }

    public async Task UseItemXpBoostTree(int userId)
    {
        var itemBag = await bagRepository.GetEquippedItemAsync(userId, ItemType.XpBoostTree);
        if (itemBag == null || itemBag.Quantity <= 0)
            return;

        itemBag.Quantity--;
        itemBag.UpdatedAt = DateTime.UtcNow;
        bagItemRepository.Update(itemBag);
        await unitOfWork.CommitAsync();
        var cacheKey = $"BagItems_{itemBag.BagId}";
        await redisService.DeleteKeyAsync(cacheKey);
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

    public async Task UseItemXpProtect(int userId)
    {
        var itemBagId = await bagRepository.GetItemByHavingUse(userId, ItemType.XpProtect);
        var itemBag = await bagItemRepository.GetByIdAsync(itemBagId);
        if (itemBag == null)
            throw new KeyNotFoundException("Item not found");
        if (itemBag.Quantity <= 0)
            throw new InvalidOperationException("Item quantity is zero");
        itemBag.Quantity--;
        itemBag.UpdatedAt = DateTime.UtcNow;
        bagItemRepository.Update(itemBag);
        await unitOfWork.CommitAsync();
        var cacheKey = $"BagItems_{itemBag.BagId}";
        await redisService.DeleteKeyAsync(cacheKey);
    }

    public async Task<string> GiftRandomItemFromListAsync(int userId)
    {
        // Get the list of giftable items  
        var giftableItems = await itemRepository.GetListItemGift();
        if (giftableItems == null || !giftableItems.Any())
            throw new InvalidOperationException("No giftable items available");

        // Randomly select an item from the list  
        var random = new Random();
        var randomItem = giftableItems[random.Next(giftableItems.Count)];

        // Check if the user has a bag  
        var bag = await bagRepository.GetByUserIdAsync(userId);
        if (bag == null)
            throw new KeyNotFoundException("User bag not found");

        // Check if the item already exists in the bag  
        var existingBagItem = await bagItemRepository.GetByBagAndItemAsync(bag.BagId, randomItem.ItemId);
        if (existingBagItem != null)
        {
            // Increment the quantity if the item already exists  
            existingBagItem.Quantity = existingBagItem.Quantity + 1;
            existingBagItem.UpdatedAt = DateTime.UtcNow;
            bagItemRepository.Update(existingBagItem);
            await unitOfWork.CommitAsync();
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
        }

        await unitOfWork.CommitAsync();

        // Invalidate the cache  
        var cacheKey = $"BagItems_{bag.BagId}";
        await redisService.DeleteKeyAsync(cacheKey);

        // Notify the user  
        await notificationService.PushNotificationAsync(userId, "Gift Item",
            $"You have received a random item: {randomItem.Name}");

        return $"Gifted {randomItem.Name} to user {userId}";
    } 










}