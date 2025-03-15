using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserTreeRepository : IGenericRepository<UserTree>
{
    Task<UserTree?> GetUserTreeDetailAsync(int userTreeId);
    Task<List<UserTree>> GetAllUserTreesAsync();
}