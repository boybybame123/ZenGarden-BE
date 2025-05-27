namespace ZenGarden.Core.Interfaces.IServices;

public interface IUseItemService
{
    Task<string> UseItemAsync(int userId, int itembagId);
    Task <int> UseItemXpBoostTree(int userId);
    Task Cancel(int bagItemId);
    Task<string> GiftRandomItemFromListAsync(int userId);
    Task<(int? ItemId, long RemainingSeconds)> GetXpBoostTreeRemainingTimeAsync(int userId);
    Task UseItemXpProtect(int userId);
}