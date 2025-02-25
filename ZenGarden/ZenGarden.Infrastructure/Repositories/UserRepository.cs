using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;
using ZenGarden.Shared.Helpers;

namespace ZenGarden.Infrastructure.Repositories;

public class UserRepository(ZenGardenContext context) : GenericRepository<Users>(context), IUserRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Users?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Users?> GetByPhoneAsync(string phone)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
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
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        user.RefreshTokenExpiry = expiryDate;

        await _context.SaveChangesAsync();
    }

    public async Task<Roles?> GetRoleByIdAsync(int roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<FilterResult<Users>> GetAllAsync(UserFilterDto filter)
    {
        IQueryable<Users> query = _context.Users
            .Include(x => x.FullName)
            .Include(x => x.Email)
            .Include(x => x.Phone)
            .Include(x => x.Status)
            .Include(x => x.Role);
        //search
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(x => x.UserId.ToString().Contains(filter.Search));
        //filter
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (Enum.TryParse<UserStatus>(filter.Status, out var status))
            {
                query = query.Where(x => x.Status == status);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.FullName))
            query = query.Where(x => x.FullName.ToString().Contains(filter.FullName));
        if (!string.IsNullOrWhiteSpace(filter.Phone))
            query = query.Where(x => x.FullName.ToString().Contains(filter.Phone));
        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(x => x.FullName.ToString().Contains(filter.Email));
        // Sort
        if (filter.UserByDescending)
            query = filter.UserByDescending switch
            {
                _ => query.OrderByDescending(x => x.CreatedAt)
            };
        else
            query = filter.UserByDescending switch
            {
                _ => query.OrderBy(x => x.CreatedAt)
            };
        var totalCount = await query.CountAsync();

        // Pagination
        var skip = (filter.PageNumber - 1) * 10;
        query = query.Skip(skip).Take(10);

        return new FilterResult<Users>
        {
            TotalCount = totalCount,
            Data = await query.ToListAsync()
        };
    }
}