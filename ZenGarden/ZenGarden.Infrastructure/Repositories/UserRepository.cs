using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
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
            .FirstOrDefaultAsync(u =>
                (!string.IsNullOrEmpty(email) && (u.Email).Equals(email, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(phone) && (u.Phone) == phone));

        if (user == null || string.IsNullOrEmpty(user.Password))
        {
            return null;
        }

        var isPasswordValid = PasswordHasher.VerifyPassword(password, user.Password);
        return !isPasswordValid ? null : user;
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
        return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = expiryDate;

        await _context.SaveChangesAsync();
    }
    
    public async Task<Roles?> GetRoleByIdAsync(int roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
    }
}