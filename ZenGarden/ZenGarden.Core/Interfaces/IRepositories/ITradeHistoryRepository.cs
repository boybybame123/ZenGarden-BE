using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITradeHistoryRepository : IGenericRepository<TradeHistory>
{
    Task<bool> IsTreeInPendingTradeAsync(int userTreeId);
    Task<List<TradeHistory>> GetAllTradeHistoriesbyStatutsAsync(int Statuts);
    Task<List<TradeHistory>> GetAllTradeHistoriesByOwneridAsync(int ownerId);
    Task<List<TradeHistory>> GetAllTradeHistoriesByNotOwneridAsync(int userId);
}