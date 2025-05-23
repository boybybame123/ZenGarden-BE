using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;
using ZenGarden.Shared.Helpers;

namespace ZenGarden.Infrastructure.Repositories;

public class UserRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<Users>(context, redisService), IUserRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Users?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Users?> GetByPhoneAsync(string? phone)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Phone == phone);
    }

    public async Task<Users?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var users = await _context.Users
            .Where(u => u.RefreshTokenHash != null)
            .ToListAsync();

        return users.FirstOrDefault(u =>
            u.RefreshTokenHash != null && PasswordHasher.VerifyPassword(refreshToken, u.RefreshTokenHash));
    }

    public async Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.RefreshTokenHash, refreshToken)
                .SetProperty(u => u.RefreshTokenExpiry, expiryDate));
    }

    public async Task<Users?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserExperience)
            .Include(u => u.Bag)
            .Include(u => u.Wallet)
            .Include(u => u.UserConfig)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<Roles?> GetRoleByIdAsync(int roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<FilterResult<Users>> GetAllAsync(UserFilterDto filter)
    {
        IQueryable<Users> query = _context.Users.Include(x => x.Role);

        // Search theo UserName
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(x => x.UserName.ToString().Contains(filter.Search));

        // Filter theo Status
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<UserStatus>(filter.Status, out var status))
            query = query.Where(x => x.Status == status);

        // Filter theo FullName, Phone, Email
        if (!string.IsNullOrWhiteSpace(filter.FullName))
            query = query.Where(x => x.UserName.Contains(filter.FullName));

        if (!string.IsNullOrWhiteSpace(filter.Phone))
            query = query.Where(x => x.Phone.Contains(filter.Phone));

        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(x => x.Email.Contains(filter.Email));

        // Sort
        query = filter.UserByDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt);

        // Tổng số bản ghi (trước khi phân trang)
        var totalCount = await query.CountAsync();

        // Pagination
        var skip = (filter.PageNumber - 1) * 10;
        var users = await query.Skip(skip).Take(10).ToListAsync();

        return new FilterResult<Users>(users, totalCount);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName)
    {
        return await _context.Users.AnyAsync(u => u.UserName.ToLower() == userName.ToLower());
    }
}