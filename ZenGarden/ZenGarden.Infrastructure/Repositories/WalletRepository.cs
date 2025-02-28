using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class WalletRepository(ZenGardenContext context) : GenericRepository<Wallet>(context), IWalletRepository;