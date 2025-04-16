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

    public async Task<ItemDto?> GetItemByIdAsync(int itemId)
    {
        var cacheKey = $"item_{itemId}";
        var cachedItem = await _redisService.GetAsync<ItemDto>(cacheKey);
        if (cachedItem != null) return cachedItem;

        var item = await _itemRepository.GetItemByIdAsync(itemId);
        if (item == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        var itemDto = _mapper.Map<ItemDto>(item);
        await _redisService.SetAsync(cacheKey, itemDto, TimeSpan.FromMinutes(30));
        return itemDto;
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
        var existingItem = await _itemRepository.GetItemByIdAsync(item.ItemId);
        if (existingItem == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found.", item.ItemId);
            throw new KeyNotFoundException($"Item with ID {item.ItemId} not found.");
        }

        existingItem.Name = item.Name ?? existingItem.Name;
        existingItem.Type = item.Type;
        existingItem.Rarity = item.Rarity ?? existingItem.Rarity;
        existingItem.Cost = item.Cost ?? existingItem.Cost;
        existingItem.Status = item.Status;
        existingItem.UpdatedAt = DateTime.Now;

        _itemRepository.Update(existingItem);
        if (await _unitOfWork.CommitAsync() == 0)
        {
            _logger.LogError("Failed to update item.");
            throw new InvalidOperationException("Failed to update item.");
        }

        await InvalidateCache();
        return existingItem;
    }

    public async Task ActiveItem(int itemId)
    {
        var itemDto = await GetItemByIdAsync(itemId);
        if (itemDto == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        var item = _mapper.Map<Item>(itemDto);
        item.Status = ItemStatus.Active;
        item.UpdatedAt = DateTime.Now;
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
        var itemDto = await GetItemByIdAsync(itemId);
        if (itemDto == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found.", itemId);
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }

        var item = _mapper.Map<Item>(itemDto);
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