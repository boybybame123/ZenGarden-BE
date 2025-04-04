using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITradeHistoryService
{
    Task<List<TradeHistory>> GetTradeHistoryAsync();
    Task<TradeHistory> GetTradeHistoryByIdAsync(int id);
    Task<TradeHistory> CreateTradeHistoryAsync(TradeHistory tradeHistory);
    Task<TradeHistory> UpdateTradeHistoryAsync(TradeHistory tradeHistory);
    
}