using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IBagItemService
{
    Task<List<BagItemDto>?> GetListItemsByBagIdAsync(int bagId);
}