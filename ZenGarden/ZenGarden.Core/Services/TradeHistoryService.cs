using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TradeHistoryService(
    ITradeHistoryRepository tradeHistoryRepository) : ITradeHistoryService
{
    public Task<TradeHistory> CreateTradeHistoryAsync(TradeHistory tradeHistory)
    {
        throw new NotImplementedException();
    }

    public Task<TradeHistory> DeleteTradeHistoryAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<TradeHistory>> GetTradeHistoryAsync()
    {
        var history = await tradeHistoryRepository.GetAllAsync();
        return history.ToList();
    }

    public Task<TradeHistory> GetTradeHistoryByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<TradeHistory> UpdateTradeHistoryAsync(TradeHistory tradeHistory)
    {
        throw new NotImplementedException();
    }
}