using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class BagItemRepository(ZenGardenContext context) : GenericRepository<BagItem>(context), IBagItemRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<BagItem?> GetByBagAndItemAsync(int bagId, int itemId)
    {
        return await _context.BagItem
            .FirstOrDefaultAsync(bi => bi.BagId == bagId && bi.ItemId == itemId);
    }

    public async Task<BagItem?> GetByIdAsync(int itembagId)
    {
        return await _context.BagItem
            .Include(b => b.Item) // Ensure the Item is included
            .FirstOrDefaultAsync(b => b.BagItemId == itembagId);
    }


    public async Task CreateRangeAsync(IEnumerable<BagItem> items)
    {
        await _context.BagItem.AddRangeAsync(items);
    }

    public async Task<bool> HasEquippedXpBoostTreeAsync(int bagId)
    {
        return await _context.BagItem
            .Include(bi => bi.Item) // Ensure Item is loaded
            .AnyAsync(bi =>
                bi.BagId == bagId &&
                bi.isEquipped &&
                bi.Item != null &&
                bi.Item.Type == ItemType.xp_boostTree);
    }
}