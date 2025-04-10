using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IBagItemRepository : IGenericRepository<BagItem>
{
    Task<BagItem?> GetByBagAndItemAsync(int bagId, int itemId);
    Task<BagItem?> GetByIdAsync(int itembagId);

}