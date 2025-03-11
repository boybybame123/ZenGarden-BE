using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Response;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITreeService
{
    Task<IEnumerable<TreeResponse>> GetAllAsync();
    Task<TreeResponse?> GetByIdAsync(int id);
    Task AddAsync(TreeDto treeDto);
    Task UpdateAsync(int id, TreeDto treeDto);
    Task DeleteAsync(int id);
}