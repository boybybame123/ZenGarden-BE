using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IBagItemRepository : IGenericRepository<BagItem>
{
    Task<BagItem?> GetByBagAndItemAsync(int bagId, int itemId);
    Task<BagItem?> GetByIdAsync(int itembagId);
    Task<List<BagItem>?> GetBagItemsByBagIdAsync(int bagId);
    Task UnequipByBagIdAndItemTypeAsync(int bagId, ItemType itemType);
}