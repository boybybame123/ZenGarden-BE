namespace ZenGarden.Core.Interfaces.IServices;

public interface IUseItemService
{
    Task<string> UseItemAsync(int userId, int itemId, int? usertreeId);
}