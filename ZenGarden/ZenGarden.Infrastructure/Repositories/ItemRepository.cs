using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories
{
    public class ItemRepository(ZenGardenContext context) : GenericRepository<Item>(context), IItemRepository
    {
        private readonly ZenGardenContext _context = context;
        public async Task<List<Item>> GetAllItemAsync()
        {
            return await _context.Item.Include(u => u.ItemDetail).ToListAsync();
        }

    }
}
