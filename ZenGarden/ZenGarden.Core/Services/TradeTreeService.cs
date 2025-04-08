using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class TradeTreeService(
    ITradeHistoryService tradeHistoryService,
    ITradeHistoryRepository tradeHistoryRepository,
    ITreeRepository treeRepository,
    IUserTreeRepository userTreeRepository,
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : ITradeTreeService
{
    public async Task<string> CreateTradeRequestAsync(TradeDto tradeDto)
    {
        try
        {
            // Validate requester's tree
            var requesterTree = await ValidateRequesterTree(tradeDto.requesterId, tradeDto.requesterTreeId);

            // Validate desired tree
            var desiredTree = await ValidateDesiredTree(tradeDto.requestDesiredTreeId, requesterTree);

            var isTreeInPendingTrade = await tradeHistoryRepository.IsTreeInPendingTradeAsync(tradeDto.requesterTreeId);
            if (isTreeInPendingTrade)
            {
                return "Your tree is already in a pending trade. Please wait for it to be completed or canceled before creating a new trade request.";
            }

            // Calculate trade fee based on rarity
            var tradeFee = CalculateTradeFee(desiredTree.Rarity);

            var userTree = await userTreeRepository.GetByIdAsync(tradeDto.requesterTreeId)
                           ?? throw new Exception("Your tree does not exist");

            // Deduct trade fee from owner's wallet
            var wallet = await walletRepository.GetByUserIdAsync(tradeDto.requesterId)
                          ?? throw new Exception("Wallet not found");

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

            await tradeHistoryRepository.CreateAsync(trade);
            await unitOfWork.CommitAsync();
            if (wallet.Balance < tradeFee)
            {
                return "Insufficient balance to create trade request.";
            }
            wallet.Balance -= tradeFee;
            walletRepository.Update(wallet);
            await unitOfWork.CommitAsync();
            return "Trade request created successfully. Waiting for recipient to accept.";
        }
        catch (Exception ex)
        {
            return $"Error creating trade request: {ex.Message}";
        }
    }
    public async Task<string> AcceptTradeAsync(int tradeId, int recipientId, int recipientTreeId)
    {
        // Validate trade exists and is pending
        var trade = await tradeHistoryService.GetTradeHistoryByIdAsync(tradeId)
                    ?? throw new Exception("Trade not found");

        if (trade.Status != TradeStatus.Pending)
            return "Trade is not in pending status";

        // Validate recipient's tree
        var recipientTree = await ValidateRecipientTree(recipientId, recipientTreeId, trade);

        // Validate requester's tree
        var requesterTree = await ValidateRequesterTreeForAcceptance(trade);

        // Deduct trade fee from recipient's wallet
        var wallet = await walletRepository.GetByUserIdAsync(recipientId)
                      ?? throw new Exception("Wallet not found");


        // Execute the trade
        await ExecuteTrade(trade, requesterTree, recipientTree);
        if (wallet.Balance < trade.TradeFee)
        {
            return "Insufficient balance to accept trade request.";
        }
        wallet.Balance -= trade.TradeFee;
        walletRepository.Update(wallet);
        await unitOfWork.CommitAsync();
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

        var TreeA = await treeRepository.GetByIdAsync(requesterTree.FinalTreeId.Value)
                     ?? throw new Exception("Original tree does not exist");

        if (desiredTree.Rarity != TreeA.Rarity)
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

        if (userTree.FinalTreeId != trade.DesiredTreeAID)
            throw new InvalidOperationException("Tree does not match the requested type");

        return userTree;
    }

    private async Task<UserTree> ValidateRequesterTreeForAcceptance(TradeHistory trade)
    {
        if (trade.TreeAid == null || trade.TreeOwnerAid == null)
            throw new InvalidOperationException("Invalid trade data");

        var requesterTree = await userTreeRepository.GetByIdAsync(trade.TreeAid)
                           ?? throw new Exception("Your tree does not exist");
        if (requesterTree.TreeOwnerId != trade.TreeOwnerAid)
            throw new Exception("This tree does not belong to you");


        return requesterTree;
    }

    private async Task ExecuteTrade(TradeHistory trade, UserTree requesterTree, UserTree recipientTree)
    {
        // Swap ownership
        var originalRequesterTreeOwnerId = requesterTree.TreeOwnerId;
        requesterTree.TreeOwnerId = recipientTree.TreeOwnerId;
        recipientTree.TreeOwnerId = originalRequesterTreeOwnerId;

        // Update trade status
        trade.Status = TradeStatus.Completed;
        trade.CompletedAt = DateTime.UtcNow;
        trade.UpdatedAt = DateTime.UtcNow;
        trade.TreeOwnerBid = recipientTree.TreeOwnerId;

        // Save changes
        userTreeRepository.Update(recipientTree);
        userTreeRepository.Update(requesterTree);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update trees");

        await tradeHistoryService.UpdateTradeHistoryAsync(trade);
    }

    #endregion
}