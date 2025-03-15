using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class BuyItemService(
    IUnitOfWork unitOfWork,
    IItemRepository itemRepository,
    IBagItemRepository bagItemRepository,
    IBagRepository bagRepository,
    IPurchaseHistoryRepository purchaseHistoryRepository,
    IItemDetailRepository itemDetailRepository,
    IWalletRepository walletRepository,
    IMapper mapper
) : IBuyItemService
{
    public async Task CreateBuyItem(BuyItemDto buyItem)
    {
        if (buyItem == null) throw new ArgumentNullException(nameof(buyItem));


        var item = await itemRepository.GetByIdAsync(buyItem.ItemId);
        var itemDetail = await itemDetailRepository.GetItemDetailsByItemId(buyItem.ItemId);
        var bag = await bagRepository.GetByIdAsync(buyItem.UserId);
        var bagItem = await bagItemRepository.GetByIdAsync(bag.BagId);

        var purchaseId = await purchaseHistoryRepository.CountAsync();

        if (bagItem == null)
            await bagItemRepository.CreateAsync(new BagItem
            {
                BagId = bag.BagId,
                ItemId = item.ItemId,
                Quantity = 0
            });

        var wallet = await walletRepository.GetByIdAsync(buyItem.UserId);

        if (item.Cost <= wallet.Balance)
        {
            wallet.Balance -= item.Cost;
            bagItem.ItemId = item.ItemId;

            bagItem.Quantity += 1;


            await purchaseHistoryRepository.CreateAsync(new PurchaseHistory
            {
                PurchaseId = purchaseId + 1,
                ItemId = item.ItemId,
                UserId = buyItem.UserId,
                TotalPrice = item.Cost,
                Status = (PurchaseHistoryStatus)1
            });


            bagItemRepository.Update(bagItem);
            walletRepository.Update(wallet);
            await unitOfWork.CommitAsync();
        }
        else
        {
            throw new InvalidOperationException("Insufficient funds");
        }
    }
}