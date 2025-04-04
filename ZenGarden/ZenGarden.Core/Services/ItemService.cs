using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class ItemService(
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IItemService
{
    public async Task<List<ItemDto>> GetAllItemsAsync()
    {
        var items = await itemRepository.GetAllItemAsync();
        try
        {
            mapper.Map<List<ItemDto>>(items);
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
        return await itemRepository.GetItemByIdAsync(itemId)
               ?? throw new KeyNotFoundException($"Item with ID {itemId} not found.");
    }

    public async Task CreateItemAsync(CreateItemDto item)
    {
        var i = mapper.Map<Item>(item);


        await itemRepository.CreateAsync(i);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create item.");
    }

    public async Task<Item> UpdateItemAsync(UpdateItemDto item)
    {
        var updateItem = await GetItemByIdAsync(item.ItemId);
        if (updateItem == null)
            throw new KeyNotFoundException($"Item with ID {item.ItemId} not found.");

        updateItem.Name = item.Name ?? updateItem.Name;
        updateItem.Type = item.Type;
        updateItem.Rarity = item.Rarity ?? updateItem.Rarity;
        updateItem.Cost = item.Cost ?? updateItem.Cost;
        updateItem.Status = item.Status;
        updateItem.UpdatedAt = DateTime.Now;


        itemRepository.Update(updateItem);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update item.");
        return updateItem;
    }


    public async Task ActiveItem(int itemId)
    {
        var item = await GetItemByIdAsync(itemId);
        if (item == null)
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");

        item.Status = ItemStatus.Active;


        itemRepository.Update(item);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete item.");
    }


    public async Task DeleteItemAsync(int itemId)
    {
        var item = await GetItemByIdAsync(itemId);


        if (item == null)
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");

        item.Status = ItemStatus.Inactive;


        itemRepository.Update(item);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete item.");
    }
}