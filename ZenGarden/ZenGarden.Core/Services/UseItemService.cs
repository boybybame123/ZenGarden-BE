﻿using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UseItemService(
    IUserConfigRepository userConfigRepository,
    IBagItemRepository bagItemRepository,
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
    }
}