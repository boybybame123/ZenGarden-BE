using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITradeTreeService
{
    Task<string> CreateTradeRequestAsync(TradeDto tradeDto);
    Task<string> AcceptTradeAsync(int tradeId, int userBId, int usertreeid);
    Task<List<TradeHistory>> GetTradeHistoryAsync();
    Task<List<TradeHistory>> GetTradeHistoryByStatusAsync(int status);
    Task<TradeHistory> GetTradeHistoryByIdAsync(int tradeId);
    Task<TradeHistory> CancelTradeAsync(int tradeId, int userA);
    Task<List<TradeHistory>> GetAllTradeHistoriesByOwneridAsync(int userId);
    Task<List<TradeHistory>> GetAllTradeHistoriesByNotOwnerIdAsync(int userId);
}