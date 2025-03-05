using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class ItemRepository(ZenGardenContext context) : GenericRepository<Item>(context), IItemRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<Item>> GetAllItemAsync()
    {
        return await _context.Item
            .Include(u => u.ItemDetail)
            .Where(u => u.ItemDetail != null) // Chỉ lấy item có ItemDetail
            .ToListAsync();
    }
}