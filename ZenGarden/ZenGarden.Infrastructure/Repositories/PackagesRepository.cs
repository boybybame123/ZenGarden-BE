using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class PackagesRepository(ZenGardenContext context, IRedisService redisService) : GenericRepository<Packages>(context, redisService), IPackagesRepository
{
}