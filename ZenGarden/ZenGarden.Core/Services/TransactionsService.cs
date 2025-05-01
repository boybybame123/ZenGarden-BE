using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class TransactionsService(ITransactionsRepository transactionsRepository, IUnitOfWork unitOfWork)
    : ITransactionsService
{
    public async Task<List<Transactions>> GetAllTransactionsByUserIdAsync(int userId)
    {
        var transactions = await transactionsRepository.ListAllTransactionsByIdAsync(userId);
        if (transactions == null) throw new KeyNotFoundException("No transactions found for this user.");
        return transactions;
    }

    public async Task<List<Transactions>> GetTransactionsAsync()
    {
        var transactions = await transactionsRepository.GetAllAsync();
        if (transactions == null) throw new KeyNotFoundException("No transactions found.");
        return transactions;
    }

    public async Task MarkOldPendingTransactionsAsFailedAsync()
    {
        // Retrieve pending transactions older than 15 minutes
        var oldPendingTransactions = await transactionsRepository.ListPendingTransactionsAsyn();
        if (oldPendingTransactions == null || !oldPendingTransactions.Any())
            // No old pending transactions to update
            return;

        // Update the status of each transaction to Failed
        foreach (var transaction in oldPendingTransactions)
        {
            transaction.Status = TransactionStatus.Failed;
            transactionsRepository.Update(transaction);
            await unitOfWork.CommitAsync();
        }

        // Save changes to the database
    }
}