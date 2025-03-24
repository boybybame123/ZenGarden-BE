using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class TradeTreeService(
    ITradeHistoryService tradehistoryRepository,
    ITreeRepository treeRepository,
    IUserTreeRepository userTreeRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : ITradeTreeService
{
    public async Task<string> CreateTradeRequestAsync(TradeDto traded)
    {
        // Get requester's tree
        var requesterTree =
            await userTreeRepository.GetUserTreeByTreeIdAndOwnerIdAsync(traded.requesterTreeId, traded.requesterId);
        if (requesterTree == null) return "Tree does not exist";


        // Get original tree information to check rating if needed later
        var treeA = await treeRepository.GetByIdAsync(requesterTree.FinalTreeId);
        if (treeA == null) return "Original tree information not found";

        var desiredTree = await treeRepository.GetByIdAsync(traded.requestDesiredTreeId);
        if (desiredTree == null) return "The tree you want to trade does not exist";
        if (!desiredTree.IsActive) return "The tree you want to trade has been deactivated";
        if (desiredTree.Rarity != treeA.Rarity) return "Rarity does not match";
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

        await tradehistoryRepository.CreateTradeHistoryAsync(trade);

        return "Trade request created successfully, waiting for recipient";
    }

    public async Task<string> AcceptTradeAsync(int tradeId, int userBId)
    {
        var trade = await tradehistoryRepository.GetTradeHistoryByIdAsync(tradeId);
        if (trade == null) return "Không tìm thấy yêu cầu trade";
        if (trade.Status != TradeStatus.Pending) return "Giao dịch không ở trạng thái chờ";
        var treechange = trade.DesiredTreeAID;
        // Lấy UserTree của B
        var userTreeB = await userTreeRepository.GetUserTreeByTreeIdAndOwnerIdAsync(treechange, userBId);

        if (userTreeB.TreeOwnerId == null) return "Cây của bạn không tồn tại";

        if (trade.TreeAid == null || trade.TreeOwnerAid == null)
            return "Thiếu dữ liệu đầu vào";
        var requesterTree =
            await userTreeRepository.GetUserTreeByTreeIdAndOwnerIdAsync(trade.TreeAid.Value, trade.TreeOwnerAid.Value);

        userTreeB.TreeOwnerId = trade.TreeOwnerAid;
        requesterTree.TreeOwnerId = userBId;
        trade.Status = TradeStatus.Completed;
        trade.CompletedAt = DateTime.UtcNow;
        trade.UpdatedAt = DateTime.UtcNow;
        trade.TreeOwnerBid = userBId;


        await tradehistoryRepository.UpdateTradeHistoryAsync(trade);
        return "Chấp nhận giao dịch thành công";
    }
}