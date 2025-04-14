using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserTreeRepository(ZenGardenContext context) : GenericRepository<UserTree>(context), IUserTreeRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<UserTree?> GetUserTreeDetailAsync(int userTreeId)
    {
        return await _context.UserTree
            .Include(ut => ut.User)
            .Include(ut => ut.FinalTree)
            .Include(ut => ut.TreeXpConfig)
            .FirstOrDefaultAsync(ut => ut.UserTreeId == userTreeId);
    }

    public async Task<List<UserTree>> GetAllUserTreesAsync()
    {
        return await _context.UserTree
            .Include(ut => ut.User)
            .Include(ut => ut.FinalTree)
            .Include(ut => ut.TreeXpConfig)
            .ToListAsync();
    }

    public async Task<List<UserTree>> GetUserTreeByUserIdAsync(int userId)
    {
        return await _context.UserTree
            .Include(ut => ut.User)
            .Include(ut => ut.FinalTree)
            .Include(ut => ut.TreeXpConfig)
            .Where(ut => ut.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserTree?> GetUserTreeByTreeIdAndOwnerIdAsync(int? treeId, int ownerId)
    {
        return await _context.UserTree
            .Include(ut => ut.User)
            .Include(ut => ut.FinalTree)
            .Include(ut => ut.TreeXpConfig)
            .Where(ut => ut.FinalTreeId == treeId && ut.TreeOwnerId == ownerId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<UserTree>> GetUserTreeByOwnerIdAsync(int ownerId)
    {
        return await _context.UserTree
            .Include(ut => ut.FinalTree)
            .Where(ut => ut.TreeOwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<List<UserTree>> GetAllActiveUserTreesAsync()
    {
        return await _context.UserTree
            .Where(ut => ut.TreeStatus == TreeStatus.Seed == true)
            .ToListAsync();
    }
}