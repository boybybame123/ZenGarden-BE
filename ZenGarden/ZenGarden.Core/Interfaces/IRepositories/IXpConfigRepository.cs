using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IXpConfigRepository : IGenericRepository<XpConfig>
{
    Task<XpConfig?> GetXpConfigAsync(int taskTypeId, int focusMethodId);
    Task<XpConfig?> GetByFocusMethodIdAndTaskTypeIdAsync(int focusMethodId, int taskTypeId);
}