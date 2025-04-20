using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserTreeRepository : IGenericRepository<UserTree>
{
    Task<UserTree?> GetUserTreeDetailAsync(int userTreeId);
    Task<List<UserTree>> GetAllUserTreesAsync();
    Task<List<UserTree>> GetUserTreeByUserIdAsync(int userId);
    Task<UserTree?> GetUserTreeByTreeIdAndOwnerIdAsync(int? treeId, int ownerId);
    Task<List<UserTree>> GetUserTreeByOwnerIdAsync(int ownerId);
    Task<List<UserTree>> GetAllActiveUserTreesAsync();
    Task<List<UserTree>> GetAllUserTreesHavingMaxLevelByOwnerIdAsync(int userId);
    Task<List<UserTree>> GetActiveUserTreeAsync(int userId);
}