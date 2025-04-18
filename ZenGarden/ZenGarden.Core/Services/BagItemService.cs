﻿using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Services;

public class BagItemService : IBagItemService
{
    private readonly IBagItemRepository _bagItemRepository;
    private readonly IMapper _mapper;
    private readonly IRedisService _redisService;
    private readonly IUnitOfWork _unitOfWork;

    public BagItemService(IBagItemRepository bagItemRepository, IUnitOfWork unitOfWork, IMapper mapper,
        IRedisService redisService)
    {
        _bagItemRepository = bagItemRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _redisService = redisService;
    }

    public async Task<List<BagItemDto>?> GetListItemsByBagIdAsync(int bagId)
    {
        var cacheKey = $"BagItems_{bagId}";
        var cachedBagItems = await _redisService.GetAsync<List<BagItemDto>>(cacheKey);

        if (cachedBagItems != null) return cachedBagItems;

        var bagItems = await _bagItemRepository.GetBagItemsByBagIdAsync(bagId);
        if (bagItems == null || !bagItems.Any())
            throw new KeyNotFoundException($"No items found for bag with ID {bagId}.");

        var bagItemDtos = _mapper.Map<List<BagItemDto>>(bagItems);
        await _redisService.SetAsync(cacheKey, bagItemDtos, TimeSpan.FromHours(1));

        return bagItemDtos;
    }
}