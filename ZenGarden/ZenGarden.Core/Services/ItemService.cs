using AutoMapper;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class ItemService(
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    INotificationService notificationService,
    IRedisService redisService,
    ILogger<ItemService> logger)
    : IItemService
{
    public async Task<List<ItemDto>> GetAllItemsAsync()
    {
        const string cacheKey = "all_items";
        var cachedItems = await redisService.GetAsync<List<ItemDto>>(cacheKey);
        if (cachedItems != null) return cachedItems;

        var items = await itemRepository.GetAllItemAsync();
        var itemDto = mapper.Map<List<ItemDto>>(items);

        await redisService.SetAsync(cacheKey, itemDto, TimeSpan.FromMinutes(30));
        return itemDto;
    }

    public async Task<ItemDto?> GetItemByIdAsync(int itemId)
    {
        var cacheKey = $"item_{itemId}";
        var cachedItem = await redisService.GetAsync<ItemDto>(cacheKey);
        if (cachedItem != null) return cachedItem;

        var item = await itemRepository.GetItemByIdAsync(itemId);
        if (item == null)
        {
            logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        var itemDto = mapper.Map<ItemDto>(item);
        await redisService.SetAsync(cacheKey, itemDto, TimeSpan.FromMinutes(30));
        return itemDto;
    }

    public async Task CreateItemAsync(CreateItemDto item)
    {
        var newItem = mapper.Map<Item>(item);
        await itemRepository.CreateAsync(newItem);
        const string cacheKey = "all_items";
        await redisService.DeleteKeyAsync(cacheKey);
        if (await unitOfWork.CommitAsync() == 0)
        {
            logger.LogError("Failed to create item.");
            throw new InvalidOperationException("Failed to create item.");
        }
        await notificationService.PushNotificationToAllAsync("Marketplace", $"Item {item.Name} has been created.");
        await InvalidateCache();
    }

    public async Task<Item> UpdateItemAsync(UpdateItemDto item)
    {
        var existingItem = await itemRepository.GetItemByIdAsync(item.ItemId);
        if (existingItem == null)
        {
            logger.LogWarning("Item with ID {ItemId} not found.", item.ItemId);
            throw new KeyNotFoundException($"Item with ID {item.ItemId} not found.");
        }

        existingItem.Name = item.Name ?? existingItem.Name;
        existingItem.Type = item.Type;
        existingItem.Rarity = item.Rarity ?? existingItem.Rarity;
        existingItem.Cost = item.Cost ?? existingItem.Cost;
        existingItem.Status = item.Status;
        existingItem.UpdatedAt = DateTime.Now;

        itemRepository.Update(existingItem);
        if (await unitOfWork.CommitAsync() == 0)
        {
            logger.LogError("Failed to update item.");
            throw new InvalidOperationException("Failed to update item.");
        }
        
        var cacheKey = $"item_{item.ItemId}";
        await redisService.DeleteKeyAsync(cacheKey);
        await InvalidateCache();
        return existingItem;
    }

    public async Task ActiveItem(int itemId)
    {
        var itemDto = await GetItemByIdAsync(itemId);
        if (itemDto == null)
        {
            logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        var item = mapper.Map<Item>(itemDto);
        item.Status = ItemStatus.Active;
        item.UpdatedAt = DateTime.Now;
        itemRepository.Update(item);
        if (await unitOfWork.CommitAsync() == 0)
        {
            logger.LogError("Failed to activate item.");
            throw new InvalidOperationException("Failed to activate item.");
        }

        var cacheKey = $"item_{itemId}";
        await redisService.DeleteKeyAsync(cacheKey);
        await InvalidateCache();
    }

    public async Task DeleteItemAsync(int itemId)
    {
        var itemDto = await GetItemByIdAsync(itemId);
        if (itemDto == null)
        {
            logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        var item = mapper.Map<Item>(itemDto);
        item.Status = ItemStatus.Inactive;
        itemRepository.Update(item);
        if (await unitOfWork.CommitAsync() == 0)
        {
            logger.LogError("Failed to delete item.");
            throw new InvalidOperationException("Failed to delete item.");
        }
        var cacheKey = $"item_{itemId}";
        await redisService.DeleteKeyAsync(cacheKey);
        await InvalidateCache();
    }

    private async Task InvalidateCache()
    {
        await redisService.DeleteKeyAsync("all_items");
    }
}