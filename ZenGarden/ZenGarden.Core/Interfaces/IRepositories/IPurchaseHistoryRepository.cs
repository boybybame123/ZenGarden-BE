using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IPurchaseHistoryRepository : IGenericRepository<PurchaseHistory>
{
    Task<int> CountPurchaseThisMonth(int userId, int itemId, DateTime startOfMonth);
}