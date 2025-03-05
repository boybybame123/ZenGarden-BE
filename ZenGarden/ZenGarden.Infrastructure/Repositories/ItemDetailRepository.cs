using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class ItemDetailRepository(ZenGardenContext context)
    : GenericRepository<ItemDetail>(context), IItemDetailRepository
{
    private readonly ZenGardenContext _context = context;


    public List<ItemDetail> GetItemDetailsByItemId(int itemId)
    {
        return _context.ItemDetail
            .Where(od => od.ItemId == itemId)
            .Include(o => o.Item)
            .ToList();
    }
}