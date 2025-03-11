using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TreeRepository(ZenGardenContext context) : GenericRepository<Tree>(context), ITreeRepository
{

}