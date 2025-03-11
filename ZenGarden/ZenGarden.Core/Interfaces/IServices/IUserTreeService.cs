using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserTreeService
{
    Task<IEnumerable<UserTree>> GetAllAsync();
    Task<UserTree> GetByIdAsync(int id);
    Task AddAsync(UserTreeDto userTreeDto);
    Task UpdateAsync(int id, UserTreeDto userTreeDto);
    Task ChangeStatusAsync(int id, TreeStatus newStatus);
}