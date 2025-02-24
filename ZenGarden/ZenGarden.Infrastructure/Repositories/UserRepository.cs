using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;
using ZenGarden.Shared.Helpers;

namespace ZenGarden.Infrastructure.Repositories;

public class UserRepository(ZenGardenContext context) : GenericRepository<Users>(context), IUserRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<Users?> ValidateUserAsync(string? email, string? phone, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Email or phone must be provided.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.");
        var user = await _context.Users
            .Include(u => u.Role) // Load th�m th�ng tin Role
            .FirstOrDefaultAsync(u =>
                (!string.IsNullOrEmpty(email) && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(phone) && u.Phone == phone));

        if (user == null || string.IsNullOrEmpty(user.Password)) return null;

        var isPasswordValid = PasswordHasher.VerifyPassword(password, user.Password);
        return isPasswordValid ? user : null;
    }

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

    public async Task<FilterResult<Users>> GetAllAsync(UserFilterDto Filter)
    {
        var totalCount = 0;
        IQueryable<Users> query = _context.Users
            .Include(x => x.FullName)
            .Include(x => x.Email)
            .Include(x => x.Phone)
            .Include(x => x.Status)
            .Include(x => x.Role);
        //search
        if (!string.IsNullOrWhiteSpace(Filter.Search))
            query = query.Where(x => x.UserId.ToString().Contains(Filter.Search));
        //filter
        if (!string.IsNullOrWhiteSpace(Filter.Status))
            query = query.Where(x => x.Status.ToString().Contains(Filter.Status));

        if (!string.IsNullOrWhiteSpace(Filter.FullName))
            query = query.Where(x => x.FullName.ToString().Contains(Filter.FullName));
        if (!string.IsNullOrWhiteSpace(Filter.Phone))
            query = query.Where(x => x.FullName.ToString().Contains(Filter.Phone));
        if (!string.IsNullOrWhiteSpace(Filter.Email))
            query = query.Where(x => x.FullName.ToString().Contains(Filter.Email));
        // Sort
        if (Filter.UserByDescending)
            query = Filter.UserByDescending switch
            {
                _ => query.OrderByDescending(x => x.CreatedAt)
            };
        else
            query = Filter.UserByDescending switch
            {
                _ => query.OrderBy(x => x.CreatedAt)
            };
        totalCount = await query.CountAsync();

        // Pagination
        var skip = (Filter.PageNumber - 1) * 10;
        query = query.Skip(skip).Take(10);

        return new FilterResult<Users>
        {
            TotalCount = totalCount,
            Data = await query.ToListAsync()
        };
    }
}