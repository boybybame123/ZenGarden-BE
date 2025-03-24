using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface ITradeTreeService
    {
        Task<string> CreateTradeRequestAsync(TradeDto tradeDto);
        Task<string> AcceptTradeAsync(int tradeId, int userBId);

    }
}
