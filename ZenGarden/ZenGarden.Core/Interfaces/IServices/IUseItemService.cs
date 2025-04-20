namespace ZenGarden.Core.Interfaces.IServices;

public interface IUseItemService
{
    Task<string> UseItemAsync(int userId, int itembagId);
    Task UseItemXpBoostTree(int userId);
    Task Cancel(int bagItemId);
    Task<string> GiftRandomItemFromListAsync(int userId);
}