using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TradeHistoryService(ITradeHistoryRepository tradeHistoryRepository, IUnitOfWork unitOfWork)
    : ITradeHistoryService
{
    public async Task<TradeHistory> CreateTradeHistoryAsync(TradeHistory tradeHistory)
    {
        ArgumentNullException.ThrowIfNull(tradeHistory);
        await tradeHistoryRepository.CreateAsync(tradeHistory);
        return tradeHistory;
    }


    public async Task<List<TradeHistory>> GetTradeHistoryAsync()
    {
        var history = await tradeHistoryRepository.GetAllAsync();
        return history.ToList();
    }

    public async Task<TradeHistory> GetTradeHistoryByIdAsync(int id)
    {
        return await tradeHistoryRepository.GetByIdAsync(id)
               ?? throw new Exception($"Trade history with ID {id} not found");
    }

    public async Task<TradeHistory> UpdateTradeHistoryAsync(TradeHistory tradeHistory)
    {
        ArgumentNullException.ThrowIfNull(tradeHistory);


        tradeHistoryRepository.Update(tradeHistory);
        if (await unitOfWork.CommitAsync() == 0)
            throw new Exception("Failed to update trade history");
        return tradeHistory;
    }






}