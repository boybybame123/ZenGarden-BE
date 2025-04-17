using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Services;

public class BagItemService(
    IBagItemRepository bagItemRepository,
    IMapper mapper,
    IRedisService redisService)
    : IBagItemService
{
    public async Task<List<BagItemDto>?> GetListItemsByBagIdAsync(int bagId)
    {
        var cacheKey = $"BagItems_{bagId}";
        var cachedBagItems = await redisService.GetAsync<List<BagItemDto>>(cacheKey);

        if (cachedBagItems != null) return cachedBagItems;

        var bagItems = await bagItemRepository.GetBagItemsByBagIdAsync(bagId);
        if (bagItems == null || !bagItems.Any())
            throw new KeyNotFoundException($"No items found for bag with ID {bagId}.");

        var bagItemDtos = mapper.Map<List<BagItemDto>>(bagItems);
        await redisService.SetAsync(cacheKey, bagItemDtos, TimeSpan.FromHours(1));

        return bagItemDtos;
    }
}