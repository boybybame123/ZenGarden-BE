using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IItemService
{
    Task ActiveItem(int itemId);
    Task<List<ItemDto>> GetAllItemsAsync();
    Task<ItemDto?> GetItemByIdAsync(int ItemId);
    Task<ItemDto> CreateItemAsync(CreateItemDto item);
    Task<Item> UpdateItemAsync(UpdateItemDto item);
    Task DeleteItemAsync(int ItemId);
}