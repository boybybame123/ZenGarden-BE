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
    public class PurchaseService: IPurchaseService
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IItemRepository _itemRepo;
        private readonly IBagRepository _bagRepo;
        private readonly IBagItemRepository _bagItemRepo;
        private readonly IItemDetailRepository _itemDetailRepo;
        IUnitOfWork _unitOfWork;


        private readonly IPurchaseHistoryRepository _purchaseHistoryRepo;

        public PurchaseService(
            IWalletRepository walletRepo,
            IItemRepository itemRepo,
            IBagRepository bagRepo,
            IBagItemRepository bagItemRepo,
            IUnitOfWork unitOfWork,
            IItemDetailRepository itemDetailRepo,
            IPurchaseHistoryRepository purchaseHistoryRepo)
        {
            _walletRepo = walletRepo;
            _itemRepo = itemRepo;
            _bagRepo = bagRepo;
            _bagItemRepo = bagItemRepo;
            _unitOfWork = unitOfWork;
            _itemDetailRepo = itemDetailRepo;
            _purchaseHistoryRepo = purchaseHistoryRepo;
        }

        public async Task<string> PurchaseItem(int userId, int itemId)
        {
            try
            {
                var wallet = await _walletRepo.GetByUserIdAsync(userId);
                if (wallet == null)
                    return "Wallet not found.";

                var item = await _itemRepo.GetByIdAsync(itemId);
                if (item == null)
                    return "Item not found.";

                if (item.Status != ItemStatus.Active)
                    return "Item is not available for purchase.";

                if (item.Cost == null || wallet.Balance < item.Cost.Value)
                    return "Insufficient balance.";

                // Trừ tiền
                wallet.Balance -= item.Cost.Value;
                _walletRepo.Update(wallet);
                await _unitOfWork.CommitAsync();
                // Ghi transaction


                // Ghi lịch sử mua với trạng thái Approved
                await _purchaseHistoryRepo.CreateAsync(new PurchaseHistory
                {
                    UserId = userId,
                    ItemId = itemId,
                    TotalPrice = item.Cost.Value,
                    CreatedAt = DateTime.UtcNow,
                    Status = PurchaseHistoryStatus.Approved
                });

                // Cộng vào bag
                var bag = await _bagRepo.GetByUserIdAsync(userId);
                if (bag == null)
                    return "Bag not found.";

                var bagItem = await _bagItemRepo.GetByBagAndItemAsync(bag.BagId, itemId);
                if (bagItem != null)
                {
                    bagItem.Quantity += 1;
                    bagItem.UpdatedAt = DateTime.UtcNow;
                     _bagItemRepo.Update(bagItem);
                    await _unitOfWork.CommitAsync();
                }
                else
                {
                    await _bagItemRepo.CreateAsync(new BagItem
                    {
                        BagId = bag.BagId,
                        ItemId = itemId,
                        Quantity = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                var itemDetail = await _itemDetailRepo.GetItemDetailsByItemId(itemId);
                if (itemDetail != null)
                {
                    itemDetail.Sold += 1;
                     _itemDetailRepo.Update(itemDetail);
                    await _unitOfWork.CommitAsync();
                }

                return "Purchase successful.";
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây
                return $"Purchase failed: {ex.Message}";
            }
        }
    }
}
