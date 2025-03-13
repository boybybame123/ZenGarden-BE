using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class ItemService(IItemRepository itemRepository, IUnitOfWork unitOfWork, IMapper mapper) : IItemService
{
    public async Task<List<ItemDto>> GetAllItemsAsync()
    {
        var items = await itemRepository.GetAllItemAsync();
        try
        {
            var i = mapper.Map<List<ItemDto>>(items);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        

        return mapper.Map<List<ItemDto>>(items);
    }


    public async Task<Item?> GetItemByIdAsync(int itemId)
    {
        return await itemRepository.GetByIdAsync(itemId)
               ?? throw new KeyNotFoundException($"Item with ID {itemId} not found.");
    }

    public async Task CreateItemAsync(Item item)
    {
        await itemRepository.CreateAsync(item);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create item.");
    }

    public async Task UpdateItemAsync(ItemDto item)
    {
        var updateItem = await GetItemByIdAsync(item.ItemId);
        if (updateItem == null)
            throw new KeyNotFoundException($"Item with ID {item.ItemId} not found.");

        mapper.Map(item, updateItem);

        itemRepository.Update(updateItem);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update item.");
    }

    public async Task DeleteItemAsync(int itemId)
    {
        var item = await GetItemByIdAsync(itemId);
        if (item == null)
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");

        await itemRepository.RemoveAsync(item);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete item.");
    }
}