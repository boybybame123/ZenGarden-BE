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

        await redisService.SetAsync(cacheKey, itemDto, TimeSpan.FromMinutes(10));
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
        try
        {
            if (item == null)
            {
                logger.LogError("CreateItemAsync called with null item");
                throw new ArgumentNullException(nameof(item));
            }

            var newItem = mapper.Map<Item>(item);
            newItem.CreatedAt = DateTime.Now;
            newItem.UpdatedAt = DateTime.Now;
            
            await itemRepository.CreateAsync(newItem);
            
            if (await unitOfWork.CommitAsync() == 0)
            {
                logger.LogError("Failed to create item: {ItemName}", item.Name);
                throw new InvalidOperationException($"Failed to create item: {item.Name}");
            }

            await notificationService.PushNotificationToAllAsync("Marketplace", $"Item {item.Name} has been released.");
            await InvalidateCache();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating item: {ItemName}", item?.Name);
            throw;
        }
    }

    public async Task<Item> UpdateItemAsync(UpdateItemDto item)
    {
        try
        {
            if (item == null)
            {
                logger.LogError("UpdateItemAsync called with null item");
                throw new ArgumentNullException(nameof(item));
            }

            var existingItem = await itemRepository.GetItemByIdAsync(item.ItemId);
            if (existingItem == null)
            {
                logger.LogWarning("Item with ID {ItemId} not found.", item.ItemId);
                throw new KeyNotFoundException($"Item with ID {item.ItemId} not found.");
            }

            // Update only non-null properties
            if (item.Name != null) existingItem.Name = item.Name;
            existingItem.Type = item.Type;
            if (item.Rarity != null) existingItem.Rarity = item.Rarity;
            if (item.Cost != null) existingItem.Cost = item.Cost;
            existingItem.Status = item.Status;
            existingItem.UpdatedAt = DateTime.Now;

            itemRepository.Update(existingItem);
            
            if (await unitOfWork.CommitAsync() == 0)
            {
                logger.LogError("Failed to update item: {ItemId}, {ItemName}", item.ItemId, existingItem.Name);
                throw new InvalidOperationException($"Failed to update item: {existingItem.Name}");
            }

            await InvalidateCache();
            await notificationService.PushNotificationToAllAsync("Marketplace", $"Item {existingItem.Name} has been updated.");
            
            return existingItem;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not ArgumentNullException)
        {
            logger.LogError(ex, "Error updating item: {ItemId}", item?.ItemId);
            throw;
        }
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