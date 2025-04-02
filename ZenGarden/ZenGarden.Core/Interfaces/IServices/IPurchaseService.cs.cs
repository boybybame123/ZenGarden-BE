namespace ZenGarden.Core.Interfaces.IServices;

public interface IPurchaseService
{
    Task<string> PurchaseItem(int userId, int itemId);
}