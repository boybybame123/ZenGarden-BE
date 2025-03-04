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
    public class ItemDetailRepository(ZenGardenContext context) : GenericRepository<ItemDetail>(context), IItemDetailRepository
    {
        private readonly ZenGardenContext _context = context;


        public List<ItemDetail> GetItemDetailsByItemId(int ItemId)
        {
            return _context.ItemDetail
                .Where(od => od.ItemId == ItemId)
                .Include(o => o.Item)
                .ToList();
        }
    }
}
