using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserXpLogRepository(ZenGardenContext context) : GenericRepository<UserXpLog>(context), IUserXpLogRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<UserXpLog?> GetUserCheckInLogAsync(int userId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        return await _context.UserXpLog
            .FirstOrDefaultAsync(x => x.UserId == userId &&
                                      x.XpSource == XpSourceType.DailyLogin &&
                                      x.CreatedAt >= startOfDay &&
                                      x.CreatedAt <= endOfDay);
    }

    public async Task<UserXpLog?> GetLastCheckInLogAsync(int userId)
    {
        return await _context.UserXpLog
            .Where(x => x.UserId == userId && x.XpSource == XpSourceType.DailyLogin)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<UserXpLog?> GetLastXpLogAsync(int userId)
    {
        return await _context.UserXpLog
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

}