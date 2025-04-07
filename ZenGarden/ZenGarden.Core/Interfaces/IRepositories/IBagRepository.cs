using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IBagRepository : IGenericRepository<Bag>
{
    Task<Bag?> GetByUserIdAsync(int userId);
    Task<bool> HasUsedXpBoostItemAsync(int userId);
    Task<int>GetItemByHavingUse(int userId, ItemType itemType);
}