using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IXpConfigRepository
{
    Task<XpConfig?> GetXpConfigAsync(int taskTypeId, int focusMethodId);
}