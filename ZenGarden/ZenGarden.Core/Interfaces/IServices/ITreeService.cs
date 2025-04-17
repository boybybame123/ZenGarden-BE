using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITreeService
{
    Task<IEnumerable<TreeDto>> GetAllAsync();
    Task<TreeDto?> GetByIdAsync(int id);
    Task<TreeDto> AddAsync(TreeDto treeDto);
    Task UpdateAsync(int id, TreeDto treeDto);
    Task<bool> DisableTreeAsync(int id);
}