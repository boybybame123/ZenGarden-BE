using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class TradeTreeService(
    ITradeHistoryService tradeHistoryRepository,
    ITreeRepository treeRepository,
    IUserTreeRepository userTreeRepository) : ITradeTreeService
{
    public async Task<string> CreateTradeRequestAsync(TradeDto traded)
    {
        // Get requester's tree

        var userTree = await userTreeRepository.GetByIdAsync(traded.requesterTreeId);

        if (userTree == null) return "Tree does not exist";
        if (userTree.TreeOwnerId != traded.requesterId) return "Tree does not belong to you";

        if (!userTree.FinalTreeId.HasValue) return "Tree is not final";


        var desiredTree = await treeRepository.GetByIdAsync(traded.requestDesiredTreeId);
        if (desiredTree == null) return "The tree you want to trade does not exist";
        if (!desiredTree.IsActive) return "The tree you want to trade has been deactivated";
        if (desiredTree.Rarity != userTree.FinalTree?.Rarity) return "Rarity does not match";
        var tradeFee = 50;
        if (desiredTree.Rarity == "Rare")
            tradeFee = 100;
        if (desiredTree.Rarity == "Super Rare")
            tradeFee = 200;
        if (desiredTree.Rarity == "Ultra Rare")
            tradeFee = 300;

        // Create trade (B and DesiredTree not yet specified)
        var trade = new TradeHistory
        {
            TreeOwnerAid = traded.requesterId,
            TreeOwnerBid = null, // B is not known yet
            TreeAid = traded.requesterTreeId,
            DesiredTreeAID = traded.requestDesiredTreeId,
            TradeFee = tradeFee,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = TradeStatus.Pending
        };

        await tradeHistoryRepository.CreateTradeHistoryAsync(trade);

        return "Trade request created successfully, waiting for recipient";
    }


    public async Task<string> AcceptTradeAsync(int tradeId, int userBId, int usertreeid)
    {
        var trade = await tradeHistoryRepository.GetTradeHistoryByIdAsync(tradeId);
        if (trade.Status != TradeStatus.Pending) return "Giao dịch không ở trạng thái chờ";
        // Lấy UserTree của B
        var userTreeB = await userTreeRepository.GetByIdAsync(usertreeid);
        if (userTreeB == null) return "Cây của bạn không tồn tại";
        if (userTreeB.TreeOwnerId != userBId) return "Cây không thuộc sở hữu của bạn";
        if (userTreeB.FinalTree != null) return "Tree is not final";
        if (userTreeB.FinalTree?.TreeId != trade.DesiredTreeAID) return "Tree does not match the desired tree";

        if (userTreeB.TreeOwnerId == null) return "Cây của bạn không tồn tại";

        if (trade.TreeAid == null || trade.TreeOwnerAid == null)
            return "Thiếu dữ liệu đầu vào";
        var requesterTree =
            await userTreeRepository.GetUserTreeByTreeIdAndOwnerIdAsync(trade.TreeAid.Value, trade.TreeOwnerAid.Value);

        userTreeB.TreeOwnerId = trade.TreeOwnerAid;
        if (requesterTree != null) requesterTree.TreeOwnerId = userBId;
        trade.Status = TradeStatus.Completed;
        trade.CompletedAt = DateTime.UtcNow;
        trade.UpdatedAt = DateTime.UtcNow;
        trade.TreeOwnerBid = userBId;


        await tradeHistoryRepository.UpdateTradeHistoryAsync(trade);
        return "Chấp nhận giao dịch thành công";
    }
}