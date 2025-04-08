using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITradeHistoryRepository : IGenericRepository<TradeHistory>
{
    Task<bool> IsTreeInPendingTradeAsync(int userTreeId);
}