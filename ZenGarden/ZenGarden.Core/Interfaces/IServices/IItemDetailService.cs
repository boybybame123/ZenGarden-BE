using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IItemDetailService
{
    Task<List<ItemDetail>> GetAllItemDetails();
    Task CreateItemDetailAsync(ItemDetail itemDetail);
    Task<ItemDetail> UpdateItemDetailAsync(UpdateItemDetailDto itemDetail);
    Task DeleteItemDetailAsync(int itemDetailId);
    string GetFolderNameByItemType(ItemType type);
}