using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITradeHistoryService
{
    Task<List<TradeHistory>> GetTradeHistoryAsync();
    Task<TradeHistory> GetTradeHistoryByIdAsync(int id);
    Task<TradeHistory> CreateTradeHistoryAsync(TradeHistory tradeHistory);
    Task<TradeHistory> UpdateTradeHistoryAsync(TradeHistory tradeHistory);
    Task<TradeHistory> DeleteTradeHistoryAsync(int id);
}
