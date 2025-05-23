﻿using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserXpLogRepository(ZenGardenContext context, IRedisService redisService) : GenericRepository<UserXpLog>(context, redisService), IUserXpLogRepository
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

    public async Task<List<UserXpLog>> GetUserCheckInsByMonthAsync(int userId, int month, int year)
    {
        return await _context.UserXpLog
            .Where(log => log.UserId == userId
                          && log.CreatedAt.Month == month
                          && log.CreatedAt.Year == year
                          && log.XpSource == XpSourceType.DailyLogin)
            .ToListAsync();
    }

    public async Task<List<UserXpLog>> GetAllUserXpLogsAsync()
    {
        return await _context.UserXpLog
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<UserXpLog>> GetUserXpLogsByUserIdAsync(int userId)
    {
        return await _context.UserXpLog
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}