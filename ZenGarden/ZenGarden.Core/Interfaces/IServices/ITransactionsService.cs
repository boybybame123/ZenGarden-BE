using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface ITransactionsService
    {
        Task<List<Transactions>> GetAllTransactionsByUserIdAsync(int userId);
    }
}
