using AutoMapper;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<ItemService> _logger;
    private readonly IMapper _mapper;
    private readonly IRedisService _redisService;
    private readonly IUnitOfWork _unitOfWork;

    public ItemService(
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IRedisService redisService,
        ILogger<ItemService> logger)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<List<ItemDto>> GetAllItemsAsync()
    {
        const string cacheKey = "all_items";
        var cachedItems = await _redisService.GetAsync<List<ItemDto>>(cacheKey);
        if (cachedItems != null) return cachedItems;

        var items = await _itemRepository.GetAllItemAsync();
        var itemDtos = _mapper.Map<List<ItemDto>>(items);

        await _redisService.SetAsync(cacheKey, itemDtos, TimeSpan.FromMinutes(30));
        return itemDtos;
    }

    public async Task<Item?> GetItemByIdAsync(int itemId)
    {
        var cacheKey = $"item_{itemId}";
        var cachedItem = await _redisService.GetAsync<Item>(cacheKey);
        if (cachedItem != null) return cachedItem;

        var item = await _itemRepository.GetItemByIdAsync(itemId);
        if (item == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        await _redisService.SetAsync(cacheKey, item, TimeSpan.FromMinutes(30));
        return item;
    }

    public async Task CreateItemAsync(CreateItemDto item)
    {
        var newItem = _mapper.Map<Item>(item);
        await _itemRepository.CreateAsync(newItem);
        if (await _unitOfWork.CommitAsync() == 0)
        {
            _logger.LogError("Failed to create item.");
            throw new InvalidOperationException("Failed to create item.");
        }

        await InvalidateCache();
    }

    public async Task<Item> UpdateItemAsync(UpdateItemDto item)
    {
        var updateItem = await GetItemByIdAsync(item.ItemId);
        if (updateItem == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found.", item.ItemId);
            throw new KeyNotFoundException($"Item with ID {item.ItemId} not found.");
        }

        updateItem.Name = item.Name ?? updateItem.Name;
        updateItem.Type = item.Type;
        updateItem.Rarity = item.Rarity ?? updateItem.Rarity;
        updateItem.Cost = item.Cost ?? updateItem.Cost;
        updateItem.Status = item.Status;
        updateItem.UpdatedAt = DateTime.Now;

        _itemRepository.Update(updateItem);
        if (await _unitOfWork.CommitAsync() == 0)
        {
            _logger.LogError("Failed to update item.");
            throw new InvalidOperationException("Failed to update item.");
        }

        await InvalidateCache();
        return updateItem;
    }

    public async Task ActiveItem(int itemId)
    {
        var item = await GetItemByIdAsync(itemId);
        if (item == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        item.Status = ItemStatus.Active;
        _itemRepository.Update(item);
        if (await _unitOfWork.CommitAsync() == 0)
        {
            _logger.LogError("Failed to activate item.");
            throw new InvalidOperationException("Failed to activate item.");
        }

        await InvalidateCache();
    }

    public async Task DeleteItemAsync(int itemId)
    {
        var item = await GetItemByIdAsync(itemId);
        if (item == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        item.Status = ItemStatus.Inactive;
        _itemRepository.Update(item);
        if (await _unitOfWork.CommitAsync() == 0)
        {
            _logger.LogError("Failed to delete item.");
            throw new InvalidOperationException("Failed to delete item.");
        }

        await InvalidateCache();
    }

    private async Task InvalidateCache()
    {
        await _redisService.DeleteKeyAsync("all_items");
    }
}