using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITransactionsService
{
    Task<List<Transactions>> GetAllTransactionsByUserIdAsync(int userId);
    Task<List<Transactions>> GetTransactionsAsync();
    Task MarkOldPendingTransactionsAsFailedAsync();
}