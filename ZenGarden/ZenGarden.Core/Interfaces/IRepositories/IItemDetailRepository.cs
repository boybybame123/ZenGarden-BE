using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IItemDetailRepository : IGenericRepository<ItemDetail>
{
    Task<ItemDetail?> GetItemDetailsByItemId(int itemId);
}