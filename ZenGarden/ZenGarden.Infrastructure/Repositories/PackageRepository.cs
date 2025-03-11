using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class PackageRepository : GenericRepository<Packages>, IPackageRepository
{
    public PackageRepository(ZenGardenContext context) : base(context)
    {
    }
}