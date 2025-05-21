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

    public async Task<ItemDto> CreateItemAsync(CreateItemDto item)
    {
        try
        {
            if (item == null)
            {
                logger.LogError("CreateItemAsync called with null item");
                throw new ArgumentNullException(nameof(item));
            }

            // Validate item details
            if (item.ItemDetail == null)
            {
                logger.LogError("Item details are required");
                throw new ArgumentException("Item details are required");
            }

            // Validate item type specific requirements
            if (item.Type == ItemType.XpBoostTree)
            {
                if (item.ItemDetail.Duration == null || item.ItemDetail.Duration <= 0)
                {
                    logger.LogError("XpBoostTree item must have a positive Duration");
                    throw new ArgumentException("XpBoostTree item must have a positive Duration");
                }

                if (string.IsNullOrWhiteSpace(item.ItemDetail.Effect) ||
                    !int.TryParse(item.ItemDetail.Effect, out var effectValue) ||
                    effectValue < 1 || effectValue > 100)
                {
                    logger.LogError("XpBoostTree item Effect must be a number between 1 and 100");
                    throw new ArgumentException("XpBoostTree item Effect must be a number between 1 and 100");
                }
            }

            // Check if item with same name exists
            var existingItem = await itemRepository.GetItemByNameAsync(item.Name);
            if (existingItem != null)
            {
                logger.LogError("Item with name {ItemName} already exists", item.Name);
                throw new ArgumentException($"Item with name {item.Name} already exists");
            }

            var newItem = mapper.Map<Item>(item);
            newItem.CreatedAt = DateTime.UtcNow;
            newItem.UpdatedAt = DateTime.UtcNow;
            newItem.Status = ItemStatus.Active; // Set initial status as Active
            
            // Initialize item detail
            if (newItem.ItemDetail != null)
            {
                newItem.ItemDetail.Sold = 0;
                newItem.ItemDetail.CreatedAt = DateTime.UtcNow;
                newItem.ItemDetail.UpdatedAt = DateTime.UtcNow;
            }
            
            await itemRepository.CreateAsync(newItem);
            
            if (await unitOfWork.CommitAsync() == 0)
            {
                logger.LogError("Failed to create item: {ItemName}", item.Name);
                throw new InvalidOperationException($"Failed to create item: {item.Name}");
            }

            // Send notification
            await notificationService.PushNotificationToAllAsync("Marketplace", $"New item {item.Name} has been released!");

            // Invalidate cache
            await InvalidateCache();

            // Return the created item
            return mapper.Map<ItemDto>(newItem);
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