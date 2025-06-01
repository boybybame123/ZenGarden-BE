using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IBagService
{
    Task<Bag?> GetBagByIdAsync(int BagId);
    Task<bool?> GetBagItemAsync(int userid);
}