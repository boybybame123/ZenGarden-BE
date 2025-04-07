using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UseItemService(
    IUserConfigRepository userConfigRepository,
    IBagItemRepository bagItemRepository,
    IBagRepository bagRepository,
    IUserXpLogRepository userXpLogRepository,
    IUserTreeRepository userTreeRepository,
    ITreeXpLogRepository treeXpLogRepository,
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
        var falg = 0;
        // ✅ Apply item vào UserConfig
        switch (item.Item.Type)
        {
            case ItemType.Background:
                userConfig.BackgroundConfig = itemDetail.MediaUrl;
                falg = await bagRepository.GetItemByHavingUse(userId, ItemType.Background);
                break;
            case ItemType.Music:
                userConfig.SoundConfig = itemDetail.MediaUrl;
                falg = await bagRepository.GetItemByHavingUse(userId, ItemType.Music);
                break;
            case ItemType.Avatar:
                userConfig.ImageUrl = itemDetail.MediaUrl;
                falg = await bagRepository.GetItemByHavingUse(userId, ItemType.Avatar);
                break;
            case ItemType.xp_boostTree:
                falg = await bagRepository.GetItemByHavingUse(userId,ItemType.xp_boostTree);

                break;
            
            default:
                return "item not rule";
        }
        if (falg > 0)
        {
            var itemOld = await bagItemRepository.GetByIdAsync(falg);
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


}