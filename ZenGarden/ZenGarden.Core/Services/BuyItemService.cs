using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services
{
    public class BuyItemService(
        IUnitOfWork unitOfWork,
        IItemRepository itemRepository,
        IBagItemRepository bagItemRepository,
        IBagRepository bagRepository,
        ITradeHistoryRepository tradeHistoryRepository,
        IItemDetailRepository itemDetailRepository,
        ITransactionsRepository transactionsRepository,
        IWalletRepository walletRepository,
        IMapper mapper
        ) : IBuyItemService
    {
        public async Task CreateBuyItem(BuyItemDto buyItem)
        {
            if (buyItem == null)
            {
                throw new ArgumentNullException(nameof(buyItem));
            }

            
            var item = await itemRepository.GetByIdAsync(buyItem.ItemId);
            var itemDetail = await itemDetailRepository.GetItemDetailsByItemId(buyItem.ItemId);
            var bag = await bagRepository.GetByIdAsync(buyItem.UserId);
            var bagItem = await bagItemRepository.GetByIdAsync(bag.BagId);
            if (bagItem == null)
            {
                await bagItemRepository.CreateAsync(new BagItem
                {
                    BagId = bag.BagId,
                    ItemId = item.ItemId,
                    Quantity = 0
                });

            }
            var wallet = await walletRepository.GetByIdAsync(buyItem.UserId);

            if (item.Cost <= wallet.Balance)
            {

                wallet.Balance -= item.Cost;
                bagItem.ItemId = item.ItemId;

                bagItem.Quantity += 1;

                var transaction = new Transactions
                {
                    TransactionId = new Random().Next(1, int.MaxValue),
                    UserId = buyItem.UserId,
                    WalletId = wallet.WalletId,
                    Amount = item.Cost,
                    
                     Type = (Domain.Enums.TransactionType)1,
                   
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };




                await transactionsRepository.CreateAsync(transaction);
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
}
