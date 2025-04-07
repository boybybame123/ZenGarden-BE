using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class TradeTreeService(
    ITradeHistoryService tradeHistoryRepository,
    ITreeRepository treeRepository,
    IUserTreeRepository userTreeRepository,
    IUnitOfWork unitOfWork)
    : ITradeTreeService
{
    public async Task<string> CreateTradeRequestAsync(TradeDto tradeDto)
    {
        // Validate requester's tree
        var requesterTree = await ValidateRequesterTree(tradeDto.requesterId, tradeDto.requesterTreeId);

        // Validate desired tree
        var desiredTree = await ValidateDesiredTree(tradeDto.requestDesiredTreeId, requesterTree);

        // Calculate trade fee based on rarity
        var tradeFee = CalculateTradeFee(desiredTree.Rarity);

        // Create and save trade request
        var trade = new TradeHistory
        {
            TreeOwnerAid = tradeDto.requesterId,
            TreeOwnerBid = null,
            TreeAid = tradeDto.requesterTreeId,
            DesiredTreeAID = tradeDto.requestDesiredTreeId,
            TradeFee = tradeFee,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = TradeStatus.Pending
        };

        await tradeHistoryRepository.CreateTradeHistoryAsync(trade);
        return "Trade request created successfully. Waiting for recipient to accept.";
    }

    public async Task<string> AcceptTradeAsync(int tradeId, int recipientId, int recipientTreeId)
    {
        // Validate trade exists and is pending
        var trade = await tradeHistoryRepository.GetTradeHistoryByIdAsync(tradeId)
                    ?? throw new Exception("Trade not found");

        if (trade.Status != TradeStatus.Pending)
            return "Trade is not in pending status";

        // Validate recipient's tree
        var recipientTree = await ValidateRecipientTree(recipientId, recipientTreeId, trade);

        // Validate requester's tree
        var requesterTree = await ValidateRequesterTreeForAcceptance(trade);

        // Execute the trade
        await ExecuteTrade(trade, requesterTree, recipientTree);

        return "Trade accepted successfully. Ownership has been transferred.";
    }

    #region Private Helper Methods

    private async Task<UserTree> ValidateRequesterTree(int requesterId, int requesterTreeId)
    {
        var userTree = await userTreeRepository.GetByIdAsync(requesterTreeId)
                       ?? throw new Exception("Tree does not exist");

        if (userTree.TreeOwnerId != requesterId)
            throw new Exception("Tree does not belong to you");

        if (!userTree.FinalTreeId.HasValue)
            throw new InvalidOperationException("Tree is not fully grown and cannot be traded");

        return userTree;
    }

    private async Task<Tree> ValidateDesiredTree(int desiredTreeId, UserTree requesterTree)
    {
        var desiredTree = await treeRepository.GetByIdAsync(desiredTreeId)
                          ?? throw new Exception("Desired tree does not exist");

        if (!desiredTree.IsActive)
            throw new InvalidOperationException("Desired tree has been deactivated");

        if (desiredTree.Rarity != requesterTree.FinalTree?.Rarity)
            throw new InvalidOperationException("Rarity levels do not match");

        return desiredTree;
    }

    private static decimal CalculateTradeFee(string rarity)
    {
        return rarity switch
        {
            "Common" => 50,
            "Rare" => 100,
            "Super Rare" => 200,
            "Ultra Rare" => 300,
            _ => 50
        };
    }

    private async Task<UserTree> ValidateRecipientTree(int recipientId, int recipientTreeId, TradeHistory trade)
    {
        var userTree = await userTreeRepository.GetByIdAsync(recipientTreeId)
                       ?? throw new Exception("Your tree does not exist");

        if (userTree.TreeOwnerId != recipientId)
            throw new Exception("This tree does not belong to you");

        if (!userTree.FinalTreeId.HasValue)
            throw new InvalidOperationException("Your tree is not fully grown");

        if (userTree.FinalTree?.TreeId != trade.DesiredTreeAID)
            throw new InvalidOperationException("Tree does not match the requested type");

        return userTree;
    }

    private async Task<UserTree> ValidateRequesterTreeForAcceptance(TradeHistory trade)
    {
        if (trade.TreeAid == null || trade.TreeOwnerAid == null)
            throw new InvalidOperationException("Invalid trade data");

        var requesterTree = await userTreeRepository.GetUserTreeByTreeIdAndOwnerIdAsync(
                                trade.TreeAid.Value, trade.TreeOwnerAid.Value)
                            ?? throw new Exception("Original tree no longer exists");

        return requesterTree;
    }

    private async Task ExecuteTrade(TradeHistory trade, UserTree requesterTree, UserTree recipientTree)
    {
        // Swap ownership
        recipientTree.TreeOwnerId = trade.TreeOwnerAid;
        requesterTree.TreeOwnerId = trade.TreeOwnerBid;

        // Update trade status
        trade.Status = TradeStatus.Completed;
        trade.CompletedAt = DateTime.UtcNow;
        trade.UpdatedAt = DateTime.UtcNow;
        trade.TreeOwnerBid = recipientTree.TreeOwnerId;

        // Save changes
        userTreeRepository.Update(recipientTree);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update trees");
        userTreeRepository.Update(requesterTree);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update trees");

        await tradeHistoryRepository.UpdateTradeHistoryAsync(trade);
    }

    #endregion
}