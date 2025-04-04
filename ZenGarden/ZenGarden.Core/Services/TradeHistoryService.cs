using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TradeHistoryService : ITradeHistoryService
{
    private readonly ITradeHistoryRepository _tradeHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TradeHistoryService(ITradeHistoryRepository tradeHistoryRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _tradeHistoryRepository = tradeHistoryRepository;
    }

    public async Task<TradeHistory> CreateTradeHistoryAsync(TradeHistory tradeHistory)
    {
        if (tradeHistory == null)
            throw new ArgumentNullException(nameof(tradeHistory));
        await _tradeHistoryRepository.CreateAsync(tradeHistory);
        return tradeHistory;
    }


    public async Task<List<TradeHistory>> GetTradeHistoryAsync()
    {
        var history = await _tradeHistoryRepository.GetAllAsync();
        return history.ToList();
    }

    public async Task<TradeHistory> GetTradeHistoryByIdAsync(int id)
    {
        return await _tradeHistoryRepository.GetByIdAsync(id)
               ?? throw new Exception($"Trade history with ID {id} not found");
    }

    public async Task<TradeHistory> UpdateTradeHistoryAsync(TradeHistory tradeHistory)
    {
        if (tradeHistory == null)
            throw new ArgumentNullException(nameof(tradeHistory));


        _tradeHistoryRepository.Update(tradeHistory);
        if (await _unitOfWork.CommitAsync() == 0)
            throw new Exception("Failed to update trade history");
        return tradeHistory;
    }
}