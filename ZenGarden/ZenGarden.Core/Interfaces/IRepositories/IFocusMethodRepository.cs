using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IFocusMethodRepository : IGenericRepository<FocusMethod>
{
    Task<FocusMethod?> GetRecommendedMethodAsync(string taskName, string? taskDescription);
}