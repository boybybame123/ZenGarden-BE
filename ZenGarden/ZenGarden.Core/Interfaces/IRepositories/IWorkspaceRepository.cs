using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IWorkspaceRepository : IGenericRepository<Workspace>
{
    public Task<Workspace?> GetByUserIdAsync(int userId);
}