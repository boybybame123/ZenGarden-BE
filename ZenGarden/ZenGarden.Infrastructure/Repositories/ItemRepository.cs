using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class ItemRepository(ZenGardenContext context) : GenericRepository<Item>(context), IItemRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<Item>> GetAllItemAsync()
    {
        return await _context.Item
            .Include(u => u.ItemDetail)
            .ToListAsync();
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await _context.Item
            .Include(u => u.ItemDetail)
            .FirstOrDefaultAsync(u => u.ItemId == id);
    }

    public async Task<List<Item>> GetListItemGift()
    {
        return await _context.Item
            .Include(u => u.ItemDetail)
            .Where(u => u.Type == ItemType.XpBoostTree || u.Type == ItemType.XpProtect)
            .ToListAsync();
    }
}