namespace ZenGarden.Core.Interfaces.IServices;

public interface IUseItemService
{
    Task<string> UseItemAsync(int userId, int itemBagId, int? userTreeId);
    Task UseItemXpBoostTree(int userId);
    Task Cancel(int bagItemId);
}