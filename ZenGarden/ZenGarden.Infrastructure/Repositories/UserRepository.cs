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
        Console.WriteLine($"📩 Email: {email}, 📞 Phone: {phone}");

        var user = _context.Users.FirstOrDefault(u => 
            (email != null && u.Email == email) || (phone != null && u.Phone == phone));

        if (user == null)
        {
            Console.WriteLine("❌ Không tìm thấy user trên Railway");
            return null;
        }

        Console.WriteLine($"🔑 Mật khẩu hash trong DB: {user.Password}");
        Console.WriteLine($"🔑 Mật khẩu nhập vào: {password}");
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
        Console.WriteLine($"✅ Kết quả kiểm tra mật khẩu: {isPasswordValid}");

        if (!isPasswordValid)
        {
            Console.WriteLine("❌ Mật khẩu không đúng");
            return null;
        }

    
        return user;
    }

}