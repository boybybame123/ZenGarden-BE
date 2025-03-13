using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IFocusMethodRepository : IGenericRepository<FocusMethod>
{
    Task<List<string>> GetMethodNamesAsync();
    Task<FocusMethod?> GetByNameAsync(string name);
}