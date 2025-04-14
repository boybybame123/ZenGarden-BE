using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class BagItemService(IBagItemRepository bagItemRepository) : IBagItemService
{
    public async Task<List<BagItem>?> GetListItemsByBagIdAsync(int bagId)
    {
        var bagItems = await bagItemRepository.GetBagItemsByBagIdAsync(bagId);
        if (bagItems == null || !bagItems.Any())
            throw new KeyNotFoundException($"No items found for bag with ID {bagId}.");
        return bagItems;
    }
}