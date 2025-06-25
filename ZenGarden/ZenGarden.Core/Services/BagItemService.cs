using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Services;

public class BagItemService(
    IBagItemRepository bagItemRepository,
    IMapper mapper,
    IRedisService redisService,
    IUserService userService)
    : IBagItemService
{
    public async Task<List<BagItemDto>?> GetListItemsByBagIdAsync(int bagId)
    {
        var cacheKey = $"BagItems_{bagId}";

        var cachedBagItems = await redisService.GetAsync<List<BagItemDto>>(cacheKey);


        if (cachedBagItems != null) return cachedBagItems;

        var bagItems = await bagItemRepository.GetBagItemsByBagIdAsync(bagId);
        if (bagItems == null || !bagItems.Any())
            return new List<BagItemDto>();

        var bagItemDtos = mapper.Map<List<BagItemDto>>(bagItems);
        await redisService.SetAsync(cacheKey, bagItemDtos, TimeSpan.FromMinutes(10));

        return bagItemDtos;
    }
    public async Task<List<BagItemDto>?> GetListItemsByUserIdAsync(int userId)
    {
        var user = await userService.GetUserByIdAsync(userId);

        if (user?.Bag == null) // Ensure user and Bag are not null
            throw new InvalidOperationException("User or user's bag not found.");

        var bagId = user.Bag.BagId;
        var cacheKey = $"BagItems_{bagId}";

        var cachedBagItems = await redisService.GetAsync<List<BagItemDto>>(cacheKey);

        if (cachedBagItems != null) return cachedBagItems;

        var bagItems = await bagItemRepository.GetBagItemsByBagIdAsync(bagId);
        if (bagItems == null || !bagItems.Any())
            return new List<BagItemDto>();

        var bagItemDtos = mapper.Map<List<BagItemDto>>(bagItems);
        await redisService.SetAsync(cacheKey, bagItemDtos, TimeSpan.FromMinutes(10));

        return bagItemDtos;
    }


}