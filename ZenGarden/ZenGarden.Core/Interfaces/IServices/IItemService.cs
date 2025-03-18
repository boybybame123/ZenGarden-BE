using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IItemService
{
    Task ActiveItem(int itemId);
    Task<List<ItemDto>> GetAllItemsAsync();
    Task<Item?> GetItemByIdAsync(int ItemId);
    Task CreateItemAsync(ItemDto item);
    Task UpdateItemAsync(UpdateItemDto item);
    Task DeleteItemAsync(int ItemId);
}