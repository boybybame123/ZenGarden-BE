using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class BagRepository(ZenGardenContext context) : GenericRepository<Bag>(context), IBagRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Bag?> GetByUserIdAsync(int userId)
    {
        return await _context.Bag.FirstOrDefaultAsync(b => b.UserId == userId);
    }

    public async Task<bool> HasUsedXpBoostItemAsync(int userId)
    {
        return await _context.Bag
            .Where(b => b.UserId == userId)
            .SelectMany(b => b.BagItem)
            .Include(bi => bi.Item)
            .AnyAsync(bi =>
                bi.isEquipped == true &&
                bi.Item != null &&
                bi.Item.Type == ItemType.xp_boostTree);
    }

    public async Task<int> GetItemByHavingUse(int userId, ItemType itemType)
    {
        var bag = await _context.Bag
            .Include(b => b.BagItem)
            .ThenInclude(bi => bi.Item)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (bag == null)
            return 0;

        var bagItem = bag.BagItem
            .FirstOrDefault(bi => bi.isEquipped && bi.Item != null && bi.Item.Type == itemType);

        if (bagItem == null)
            throw new KeyNotFoundException("BagItem not found.");

        return bagItem.BagItemId;
    }
    public async Task UnequipZeroQuantityItems(int userId)
    {
   
        var itemsToUnequip = await _context.BagItem
            .Where(bi => bi.Bag.UserId == userId
                        && bi.isEquipped
                        && bi.Quantity == 0)
            .ToListAsync();

     
        foreach (var item in itemsToUnequip)
        {
            item.isEquipped = false;
        }

      
        await _context.SaveChangesAsync();
    }
}