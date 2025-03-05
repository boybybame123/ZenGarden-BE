using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IItemService
{
    Task<List<ItemDto>> GetAllItemsAsync();
    Task<Item?> GetItemByIdAsync(int ItemId);
    Task CreateItemAsync(Item item);
    Task UpdateItemAsync(ItemDto item);
    Task DeleteItemAsync(int ItemId);
}