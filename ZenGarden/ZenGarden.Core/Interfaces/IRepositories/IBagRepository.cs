using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IBagRepository : IGenericRepository<Bag>
{
    Task<Bag?> GetByUserIdAsync(int userId);
}