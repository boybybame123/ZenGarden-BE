using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITreeRepository : IGenericRepository<Tree>
{
    Task<List<int>> GetAllTreeIdsAsync();
}