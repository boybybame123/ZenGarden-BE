using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IBagItemService
{
    Task<List<BagItemDto>?> GetListItemsByBagIdAsync(int bagId);
    Task<List<BagItemDto>?> GetListItemsByUserIdAsync(int userId);
}