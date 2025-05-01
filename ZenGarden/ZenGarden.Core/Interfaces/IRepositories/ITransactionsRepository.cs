using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITransactionsRepository : IGenericRepository<Transactions>
{
    Task<Transactions?> FindByRefAsync(string transactionRef);
    Task<List<Transactions>?> ListAllTransactionsByIdAsync(int userId);
    Task<List<Transactions>?> ListPendingTransactionsAsyn();
}