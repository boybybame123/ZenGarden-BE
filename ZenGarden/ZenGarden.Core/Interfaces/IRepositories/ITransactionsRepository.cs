using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITransactionsRepository : IGenericRepository<Transactions>
{
    Task<Transactions?> FindByRefAsync(string transactionRef);
}