using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserTreeService
{
    Task AddAsync(CreateUserTreeDto createUserTreeDto);
    Task UpdateAsync(int id, CreateUserTreeDto createUserTreeDto);
    Task ChangeStatusAsync(int id, TreeStatus newStatus);
    Task<List<UserTreeDto>> GetAllUserTreesAsync();
    Task<UserTreeDto> GetUserTreeDetailAsync(int userTreeId);
    Task CheckAndSetMaxLevelAsync(UserTree userTree);
    Task<List<UserTreeDto>> GetAllUserTreesByUserIdAsync(int userid);
    Task UpdateSpecificTreeHealthAsync(int userTreeId);
    Task<List<UserTree>> ListUserTreeByOwner(int ownerId);
    Task<List<UserTree>> GetAllUserTreesHavingMaxLevelByOwnerIdAsync(int userId);
    Task<List<UserTreeDto>> GetActiveUserTreeAsync(int userId);
}