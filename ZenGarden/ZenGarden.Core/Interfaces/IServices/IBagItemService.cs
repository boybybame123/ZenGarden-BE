using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IBagItemService
{
    Task<List<BagItem>?> GetListItemsByBagIdAsync(int bagId);
}