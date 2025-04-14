using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UseItemService(
    IUserConfigRepository userConfigRepository,
    IBagItemRepository bagItemRepository,
    IItemDetailRepository itemDetailRepository,
    IBagRepository bagRepository,
    IUnitOfWork unitOfWork
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

        var itemDetail = await itemDetailRepository.GetItemDetailsByItemId(item.ItemId!.Value);
        if (itemDetail == null)
            throw new KeyNotFoundException("Item detail not found");

        if (item.Item == null)


            throw new KeyNotFoundException("Item details not found");

        var userConfig = await userConfigRepository.GetByIdAsync(userId);
        if (userConfig == null)
            throw new KeyNotFoundException("User config not found");


        int flag;
        // ✅ Apply item vào UserConfig
        switch (item.Item.Type)
        {
            case ItemType.Background:
                userConfig.BackgroundConfig = itemDetail.MediaUrl;
                flag = await bagRepository.GetItemByHavingUse(userId, ItemType.Background);
                break;
            case ItemType.Music:
                userConfig.SoundConfig = itemDetail.MediaUrl;
                flag = await bagRepository.GetItemByHavingUse(userId, ItemType.Music);
                break;
            case ItemType.Avatar:
                userConfig.ImageUrl = itemDetail.MediaUrl;
                flag = await bagRepository.GetItemByHavingUse(userId, ItemType.Avatar);
                break;
            case ItemType.xp_boostTree:
                flag = await bagRepository.GetItemByHavingUse(userId, ItemType.xp_boostTree);
                break;
            case ItemType.Xp_protect:
                flag = await bagRepository.GetItemByHavingUse(userId, ItemType.Xp_protect);
                break;
            default:
                return "item not rule";
        }

        if (flag > 0)
        {
            var itemOld = await bagItemRepository.GetByIdAsync(flag);
            if (itemOld != null)
            {
                itemOld.isEquipped = false;
                bagItemRepository.Update(itemOld);
            }
        }


        item.isEquipped = true;
        userConfig.UpdatedAt = DateTime.UtcNow;

        userConfigRepository.Update(userConfig);
        bagItemRepository.Update(item);
        await unitOfWork.CommitAsync();

        return "Sử dụng item thành công";
    }

    public async Task UseItemXpBoostTree(int userId)
    {
        var itemBag = await bagRepository.GetEquippedItemAsync(userId, ItemType.xp_boostTree);
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
        var itemBagId = await bagRepository.GetItemByHavingUse(userId, ItemType.Xp_protect);
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