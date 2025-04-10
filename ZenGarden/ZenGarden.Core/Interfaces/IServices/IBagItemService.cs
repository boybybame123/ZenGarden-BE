using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface IBagItemService
    {
        Task<List<BagItem>?> GetListItemsByBagIdAsync(int bagId);
    }
}
