namespace ZenGarden.Core.Interfaces.IServices;

public interface IXpConfigService
{
    Task EnsureXpConfigExists(int focusMethodId, int taskTypeId, int totalDuration);
}