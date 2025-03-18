using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class BagItemRepository(ZenGardenContext context) : GenericRepository<BagItem>(context), IBagItemRepository
{
    public async Task<BagItem?> GetByBagAndItemAsync(int bagId, int itemId)
    {
        return await context.BagItem
            .FirstOrDefaultAsync(bi => bi.BagId == bagId && bi.ItemId == itemId);
    }
}
