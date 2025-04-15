using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IBagItemService
{
    Task<List<BagItemDto>?> GetListItemsByBagIdAsync(int bagId);
}