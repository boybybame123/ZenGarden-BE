using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UseItemService(
    IUserConfigRepository userConfigRepository,
    IBagItemRepository bagItemRepository,
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
        if (item.Item.Type is ItemType.Xp_boostUser or ItemType.xp_boostTree)
            percent = double.TryParse(itemDetail.Effect, out var effect) ? effect : 0;

        if (percent <= 0)
            return "Hiệu ứng item không hợp lệ";

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
            case ItemType.xp_boostTree:
                var treeXpResult = await AddXpByPercentAsync(userId, usertreeId ?? 0, percent, 1);
                if (!treeXpResult.success) return treeXpResult.message;
                item.Quantity -= 1;
                break;
            case ItemType.Xp_boostUser:
                var userXpResult = await AddXpByPercentAsync(userId, usertreeId ?? 0, percent, 2);
                if (!userXpResult.success) return userXpResult.message;
                item.Quantity -= 1;
                break;
            case ItemType.Xp_protect:
            default:
                return "Loại item không hợp lệ";
        }

        item.isEquipped = true;
        userConfig.UpdatedAt = DateTime.UtcNow;

        userConfigRepository.Update(userConfig);
        bagItemRepository.Update(item);
        await unitOfWork.CommitAsync();

        return "Sử dụng item thành công";
    }

    private async Task<(bool success, string message)> AddXpByPercentAsync(int userId, int userTreeId, double percent,
        int flag)
    {
        switch (flag)
        {
            // ✅ Tree XP
            case 1:
            {
                var userTree = await userTreeRepository.GetByIdAsync(userTreeId);
                if (userTree == null) return (false, "Không tìm thấy cây hoặc bạn không sở hữu cây");


                var latestTreeLog = await treeXpLogRepository.GetLatestTreeXpLogByUserTreeIdAsync(userTreeId);
                if (latestTreeLog == null) return (false, "Không tìm thấy log XP của cây");

                var xpToAdd = latestTreeLog.XpAmount * percent / 100;
                if (xpToAdd <= 0) return (false, "XP không hợp lệ");

                userTree.TotalXp += xpToAdd;
                userTree.UpdatedAt = DateTime.UtcNow;

                var treeXpLog = new TreeXpLog
                {
                    XpAmount = xpToAdd,
                    ActivityType = ActivityType.ItemXp,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await treeXpLogRepository.CreateAsync(treeXpLog);
                userTreeRepository.Update(userTree);
                await unitOfWork.CommitAsync();

                return (true, $"Đã cộng {xpToAdd} XP vào cây thành công");
            }
            // ✅ User XP
            case 2:
            {
                var latestUserXpLog = await userXpLogRepository.GetLastXpLogAsync(userId);
                if (latestUserXpLog == null) return (false, "Không tìm thấy log XP của người dùng");

                var xpToAdd = latestUserXpLog.XpAmount * percent / 100;
                if (xpToAdd <= 0) return (false, "XP không hợp lệ");

                var userXpLog = new UserXpLog
                {
                    UserId = userId,
                    XpAmount = xpToAdd,
                    XpSource = XpSourceType.ItemBoost,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await userXpLogRepository.CreateAsync(userXpLog);
                return (true, $"Đã cộng {xpToAdd} XP vào User thành công");
            }
            default:
                return (false, "Flag không hợp lệ");
        }
    }
}