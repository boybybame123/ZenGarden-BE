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
            .Include(i => i.ItemDetail)
            .ToListAsync();
    }

    public async Task<Item?> GetItemByIdAsync(int itemId)
    {
        return await _context.Item
            .Include(i => i.ItemDetail)
            .FirstOrDefaultAsync(i => i.ItemId == itemId);
    }

    public async Task<Item?> GetItemByNameAsync(string name)
    {
        return await _context.Item
            .Include(i => i.ItemDetail)
            .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower());
    }

    public async Task<List<Item>> GetListItemGift()
    {
        return await _context.Item
            .Include(u => u.ItemDetail)
            .Where(u => u.Type == ItemType.XpBoostTree || u.Type == ItemType.XpProtect)
            .ToListAsync();
    }
}