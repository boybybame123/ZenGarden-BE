using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITradeTreeService
{
    Task<string> CreateTradeRequestAsync(TradeDto tradeDto);
    Task<string> AcceptTradeAsync(int tradeId, int userBId, int usertreeid);
}