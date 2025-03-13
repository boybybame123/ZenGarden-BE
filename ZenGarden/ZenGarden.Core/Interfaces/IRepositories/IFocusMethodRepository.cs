using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IFocusMethodRepository : IGenericRepository<FocusMethod>
{
    Task<(FocusMethod? suggestedMethod, List<FocusMethod> availableMethods)> GetRecommendedMethodAsync(
        string taskName,
        string? taskDescription,
        DateTime startDate,
        DateTime endDate);
    Task<FocusMethod?> GetByIdAsync(int focusMethodId);
}