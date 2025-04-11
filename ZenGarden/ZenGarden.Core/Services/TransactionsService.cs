
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services
{
    public class TransactionsService(ITransactionsRepository transactionsRepository, IUnitOfWork unitOfWork) : ITransactionsService
    {
        public async Task<List<Transactions>> GetAllTransactionsByUserIdAsync(int userId)
        {
            var transactions = await transactionsRepository.ListAllTransactionsByIdAsync(userId);
            if (transactions == null)
            {
                throw new KeyNotFoundException("No transactions found for this user.");
            }
            return transactions;
        }
    }
}
