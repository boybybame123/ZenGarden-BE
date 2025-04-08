namespace ZenGarden.Core.Interfaces.IServices;

public interface IUseItemService
{
    Task<string> UseItemAsync(int userId, int itembagId, int? usertreeId);
    Task UseItemXpBoostTree(int userId);
    Task Cancel(int bagitemid);
}