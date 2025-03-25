using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UserXpLogService(
    IUserXpLogRepository userXpLogRepository,
    IUserExperienceRepository userExperienceRepository,
    IMapper mapper)
    : IUserXpLogService
{
    public async Task<UserXpLogDto?> GetUserCheckInLogAsync(int userId, DateTime date)
    {
        var userXpLog = await userXpLogRepository.GetUserCheckInLogAsync(userId, date);

        return userXpLog == null ? null : mapper.Map<UserXpLogDto>(userXpLog);
    }


    public async Task<double> CheckInAndGetXpAsync(int userId)
    {
        const int xpBase = 10;
        const double streakBonusRate = 0.1;
        const int maxStreakDays = 7;

        var userExp = await userExperienceRepository.GetByUserIdAsync(userId);
        if (userExp == null)
            throw new KeyNotFoundException("User experience record not found.");

        var lastCheckIn = await userXpLogRepository.GetLastCheckInLogAsync(userId);

        int streakDays;
        if (lastCheckIn != null)
        {
            var daysSinceLastCheckIn = (DateTime.UtcNow.Date - lastCheckIn.CreatedAt.Date).Days;
            streakDays = daysSinceLastCheckIn == 1 ? Math.Min(userExp.StreakDays + 1, maxStreakDays) : 1;
        }
        else
        {
            streakDays = 1;
        }

        var streakMultiplier = 1 + ((streakDays - 1) * streakBonusRate);
        var xpEarned = xpBase * streakMultiplier;

        userExp.TotalXp += xpEarned;
        userExp.StreakDays = streakDays;
        userExp.UpdatedAt = DateTime.UtcNow;
        userExperienceRepository.Update(userExp);

        var log = new UserXpLog
        {
            UserId = userId,
            XpAmount = xpEarned,
            XpSource = XpSourceType.DailyLogin,
            CreatedAt = DateTime.UtcNow
        };

        await userXpLogRepository.CreateAsync(log);

        return xpEarned;
    }

}