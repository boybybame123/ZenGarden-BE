using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services
{
    public class UseItemService(IUserConfigRepository userConfigRepository, IBagItemRepository bagItemRepository ,IUserXpLogRepository userXpLogRepository,IUserTreeRepository userTreeRepository , ITreeXpLogRepository treeXpLogRepository

        ) : IUseItemService
    {
        public async Task<string> UseItemAsync(int userId, int itemId, int usertreeId)
        {
            // Lấy item + item detail
            var item = await bagItemRepository.GetByIdAsync(itemId);
            if (item == null )
                return "Item không tồn tại";

            var itemDetail = item.Item.ItemDetail;



            // Lấy 
            var userConfig = await userConfigRepository.GetByIdAsync(userId);
            if (userConfig == null)
                return "UserConfig không tồn tại";
            
            
            double percent = 0;

            if (itemDetail.Type =="TreeXp"|| itemDetail.Type == "UserXp")
            {
                percent = double.TryParse(itemDetail.Effect, out var effect) ? effect : 0;
            }




            // ✅ Apply item vào UserConfig
            switch (itemDetail.Type)
            {
                case "background":
                    userConfig.BackgroundConfig = itemDetail.MediaUrl;
                    break;
                case "music":
                    userConfig.SoundConfig = itemDetail.MediaUrl;
                    break;
                case "image":
                    userConfig.ImageUrl = itemDetail.MediaUrl;
                    break;
                case "TreeXP":
                     await AddXpByPercentAsync(userId, usertreeId, percent, 1);
                    item.Quantity -= 1;
                    break;
                case "UserXp":
                    await AddXpByPercentAsync(userId, usertreeId, percent, 2);
                    item.Quantity -= 1;
                    break;
                default:
                    return "Loại item không hợp lệ";
            }

            item.isEquipped = true;
            userConfig.UpdatedAt = DateTime.UtcNow;
            await userConfigRepository.UpdateAsync(userConfig);
            await bagItemRepository.UpdateAsync(item);

            return "Sử dụng item thành công";


        }

        public async Task<string> AddXpByPercentAsync(int userId, int userTreeId, double percent, int flag)
        {
            if (flag == 1) // ✅ Tree XP
            {
                var userTree = await userTreeRepository.GetByIdAsync(userTreeId);
                if (userTree == null) return "Không tìm thấy cây hoặc bạn không sở hữu cây";

                var latestTreeLog = await treeXpLogRepository.GetLatestTreeXpLogByUserTreeIdAsync(userTreeId);
                if (latestTreeLog == null) return "Không tìm thấy log XP của cây";

                // ✅ Tính % XP từ log gần nhất
                double xpToAdd = latestTreeLog.XpAmount * percent / 100;
                userTree.TotalXp += xpToAdd;
                userTree.UpdatedAt = DateTime.UtcNow;

                // ✅ Ghi log cho cây
                var treeXpLog = new TreeXpLog
                {
                    XpAmount = xpToAdd,
                    ActivityType = ActivityType.ItemXp,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await treeXpLogRepository.CreateAsync(treeXpLog);
                await userTreeRepository.UpdateAsync(userTree);

                return $"Đã cộng {xpToAdd} XP vào cây thành công";
            }
            else if (flag == 2) // ✅ User XP
            {
                var latestUserXpLog = await  userXpLogRepository.GetLastXpLogAsync(userId);
                if (latestUserXpLog == null) return "Không tìm thấy log XP của người dùng";

                double xpToAdd = latestUserXpLog.XpAmount * percent / 100;

                // ✅ Ghi UserXpLog
                var userXpLog = new UserXpLog
                {
                    UserId = userId,
                    XpAmount = xpToAdd,
                    XpSource = XpSourceType.ItemBoost,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await userXpLogRepository.CreateAsync(userXpLog);

                return $"Đã cộng {xpToAdd} XP vào User thành công";
            }

            return "Flag không hợp lệ";
        }










    }
}
