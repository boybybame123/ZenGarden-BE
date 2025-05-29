using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IFocusMethodRepository : IGenericRepository<FocusMethod>
{
    Task<List<string>> GetMethodNamesAsync();
    Task<FocusMethod?> SearchBySimilarityAsync(string methodName);
    Task<FocusMethodDto?> GetDtoByIdAsync(int id);
    Task<FocusMethod?> GetByNameAsync(string? name);
}