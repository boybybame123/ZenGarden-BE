using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ZenGardenContext _context;
    public UserRepository(ZenGardenContext context)
    {
        _context = context;
    }

    public Users? ValidateUser(string? email, string? phone, string password)
    {
        var user = _context.Users.FirstOrDefault(u => 
            (email != null && u.Email == email) || (phone != null && u.Phone == phone));

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return null;
        }
        return user;
    }

}