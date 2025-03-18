using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class ItemDetailRepository(ZenGardenContext context): GenericRepository<ItemDetail>(context), IItemDetailRepository
{
    private readonly ZenGardenContext _context = context;


    public async Task<ItemDetail?> GetItemDetailsByItemId(int itemId)
    {
        return await _context.ItemDetail
            .FirstOrDefaultAsync(od => od.ItemId == itemId);
    }
}