using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IItemDetailService
{
    Task<List<ItemDetailDto>> GetAllItemDetails();
    Task CreateItemDetailAsync(ItemDetail itemDetail);
    Task UpdateItemDetailAsync(UpdateItemDetailDto itemDetail);
    Task DeleteItemDetailAsync(int itemDetailId);
}