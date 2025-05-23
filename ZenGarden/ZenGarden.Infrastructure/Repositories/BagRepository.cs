using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class BagRepository(ZenGardenContext context, IRedisService redisService) : GenericRepository<Bag>(context, redisService), IBagRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Bag?> GetByUserIdAsync(int userId)
    {
        return await _context.Bag.FirstOrDefaultAsync(b => b.UserId == userId);
    }

    public async Task<bool> HasUsedXpBoostItemAsync(int userId)
    {
        return await _context.Bag
            .Where(b => b.UserId == userId)
            .SelectMany(b => b.BagItem)
            .Include(bi => bi.Item)
            .AnyAsync(bi =>
                bi.isEquipped == true &&
                bi.Item != null &&
                bi.Item.Type == ItemType.XpBoostTree);
    }

    public async Task<int> GetItemByHavingUse(int userId, ItemType itemType)
    {
        var bag = await _context.Bag
            .Include(b => b.BagItem)
            .ThenInclude(bi => bi.Item)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        var bagItem = bag?.BagItem
            .FirstOrDefault(bi => bi.isEquipped && bi.Item != null && bi.Item.Type == itemType);

        return bagItem?.BagItemId ?? 0;
    }

    public async Task UnequipZeroQuantityItems(int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var affectedRows = await _context.BagItem
                .Where(bi => bi.Bag.UserId == userId &&
                             bi.isEquipped &&
                             bi.Quantity == 0)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(bi => bi.isEquipped, false));

            if (affectedRows == 0) throw new InvalidOperationException("No items found to unequip.");

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Failed to unequipped items for user {userId}.", ex);
        }
    }

    public async Task<BagItem?> GetEquippedItemAsync(int userId, ItemType itemType)
    {
        var bag = await _context.Bag
            .Include(b => b.BagItem)
            .ThenInclude(bi => bi.Item)
            .ThenInclude(i => i.ItemDetail)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        return bag?.BagItem
            .FirstOrDefault(bi => bi.isEquipped && bi.Item?.Type == itemType);
    }

    public async Task<bool> UnequipAllZeroQuantityItems()
    {
        var hasItems = await _context.BagItem
            .AnyAsync(bi => bi.isEquipped && bi.Quantity == 0);

        if (!hasItems) return false;

        await _context.BagItem
            .Where(bi => bi.isEquipped && bi.Quantity == 0)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(bi => bi.isEquipped, false)
            );

        return true;
    }
}