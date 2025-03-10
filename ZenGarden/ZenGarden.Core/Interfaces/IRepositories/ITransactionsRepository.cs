using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories
{
    public interface ITransactionsRepository : IGenericRepository<Transactions>
    {
        Task<Transactions?> FindByRefAsync(string transactionRef);
    }
}