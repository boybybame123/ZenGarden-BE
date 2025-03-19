using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserTreeService
{
    Task AddAsync(CreateUserTreeDto createUserTreeDto);
    Task UpdateAsync(int id, CreateUserTreeDto createUserTreeDto);
    Task ChangeStatusAsync(int id, TreeStatus newStatus);
    Task<List<UserTreeDto>> GetAllUserTreesAsync();
    Task<UserTreeDto> GetUserTreeDetailAsync(int userTreeId);
    Task<UserTreeDto> GetUserTreeByUserIdAsync(int userId);
}