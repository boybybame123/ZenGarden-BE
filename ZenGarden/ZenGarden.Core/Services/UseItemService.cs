using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UseItemService(
    IUserConfigRepository userConfigRepository,
    IBagItemRepository bagItemRepository,
    IBagRepository bagRepository,
    IUnitOfWork unitOfWork
) : IUseItemService
{
    public async Task<string> UseItemAsync(int userId, int itembagId, int? usertreeId)
    {
        var item = await bagItemRepository.GetByIdAsync(itembagId);
        if (item == null || item.Quantity <= 0)
            return "Item không tồn tại hoặc đã hết số lượng";

        var itemDetail = item.Item.ItemDetail;
        var userConfig = await userConfigRepository.GetByIdAsync(userId);
        if (userConfig == null)
            return "UserConfig không tồn tại";

        double percent = 0;
        if (item.Item.Type is ItemType.xp_boostTree)
            percent = double.TryParse(itemDetail.Effect, out var effect) ? effect : 0;

        if (percent <= 0)
            return "Hiệu ứng item không hợp lệ";
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

        return "Use Item successfully";
    }

    public async Task UseItemXpBoostTree(int userId)
    {
        var itemBagId = await bagRepository.GetItemByHavingUse(userId, ItemType.xp_boostTree);
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

    public async Task Cancel(int bagitemid)
    {
        var item = await bagItemRepository.GetByIdAsync(bagitemid);
        if (item == null)
            throw new KeyNotFoundException("Item not found");
        item.isEquipped = false;
        item.UpdatedAt = DateTime.UtcNow;
        bagItemRepository.Update(item);
        await unitOfWork.CommitAsync();
    }
}