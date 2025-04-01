using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
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
    public async Task CreateRangeAsync(IEnumerable<BagItem> items)
    {
        await _context.BagItem.AddRangeAsync(items);
    }
}