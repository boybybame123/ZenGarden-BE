using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services
{
    public class PaymentService
    {
        private readonly ITransactionsRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            ITransactionsRepository transactionRepository,
            IWalletRepository walletRepository,
            IPackageRepository packageRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _packageRepository = packageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreatePaymentIntent(CreatePaymentRequest request)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null || !package.IsActive)
                throw new Exception("Gói nạp không hợp lệ.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(package.Price * 100), // Đơn vị: cent
                Currency = "vnd",
                PaymentMethodTypes = new List<string> { "card" }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            var transaction = new Transactions
            {
                UserId = request.UserId,
                WalletId = request.WalletId,
                PackageId = request.PackageId,
                Amount = package.Amount,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Pending,
                PaymentMethod = "Stripe",
                TransactionRef = paymentIntent.Id
            };

            await _transactionRepository.CreateAsync(transaction);
            return paymentIntent.ClientSecret;
        }

        public async Task HandlePaymentSucceeded(string paymentIntentId)
        {
            var transaction = await _transactionRepository.FindByRefAsync(paymentIntentId);
            if (transaction != null && transaction.Status == TransactionStatus.Pending)
            {
                transaction.Status = TransactionStatus.Completed;
                transaction.CompletedAt = DateTime.UtcNow;

                var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
                if (wallet != null)
                {
                    wallet.Balance += transaction.Amount ?? 0;
                    wallet.UpdatedAt = DateTime.UtcNow;
                    _walletRepository.Update(wallet);
                    await _unitOfWork.CommitAsync();
                }

                _transactionRepository.Update(transaction);
                await _unitOfWork.CommitAsync();
            }
        }
    }
}
