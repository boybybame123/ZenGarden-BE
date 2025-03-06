using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class PackagesRepository(ZenGardenContext context) : GenericRepository<Packages>(context), IPackagesRepository
{
}