using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IItemRepository : IGenericRepository<Item>
{
    Task<List<Item>> GetAllItemAsync();
    Task<Item?> GetItemByIdAsync(int itemId);
    Task<Item?> GetItemByNameAsync(string name);
    Task<List<Item>> GetListItemGift();
}