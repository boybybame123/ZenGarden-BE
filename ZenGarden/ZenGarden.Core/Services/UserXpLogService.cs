using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UserXpLogService(
    IUserXpLogRepository userXpLogRepository,
    IUserExperienceRepository userExperienceRepository,
    INotificationService notificationService,
    IUnitOfWork unitOfWork)
    : IUserXpLogService
{
    public async Task<List<DateTime>> GetUserCheckInHistoryAsync(int userId, int month, int year)
    {
        var checkIns = await userXpLogRepository.GetUserCheckInsByMonthAsync(userId, month, year);

        return checkIns.Select(log => log.CreatedAt.Date).Distinct().ToList();
    }


    public async Task<(double xpEarned, string notificationMessage)> CheckInAndGetXpAsync(int userId)
    {
        var xpBase = GetXpAmountBySource(XpSourceType.DailyLogin);
        const double streakBonusRate = 0.1;
        const int maxStreakDays = 7;

        var today = DateTime.UtcNow.Date;

        var existingCheckIn = await userXpLogRepository.GetUserCheckInLogAsync(userId, today);
        if (existingCheckIn != null)
            return (0, "You already checked in today!");

        var userExp = await userExperienceRepository.GetByUserIdAsync(userId);
        if (userExp == null)
        {
            userExp = new UserExperience
            {
                UserId = userId,
                TotalXp = 0,
                StreakDays = 0,
                UpdatedAt = DateTime.UtcNow
            };

            await userExperienceRepository.CreateAsync(userExp);
        }

        var lastCheckIn = await userXpLogRepository.GetLastCheckInLogAsync(userId);

        var consecutiveDays = userExp.StreakDays; 
        var streakDays = 0; 

        if (lastCheckIn != null)
        {
            var daysSinceLastCheckIn = (DateTime.UtcNow.Date - lastCheckIn.CreatedAt.Date).Days;

            consecutiveDays = daysSinceLastCheckIn switch
            {
                1 => consecutiveDays + 1,
                > 1 => 0,
                _ => consecutiveDays
            };

            if (consecutiveDays >= 3)
                streakDays = Math.Min(consecutiveDays - 2, maxStreakDays); 
        }

        var streakMultiplier = 1 + (streakDays - 1) * streakBonusRate;
        var xpEarned = xpBase * streakMultiplier;

        userExp.TotalXp += xpEarned;
        userExp.StreakDays = consecutiveDays;
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
        await unitOfWork.CommitAsync();

        var message = $"You checked in and earned {xpEarned:F0} XP!";
        await notificationService.PushNotificationAsync(userId, "Daily Check-in", message);

        return (xpEarned, message);
    }
    
    public async Task<int> GetCurrentStreakAsync(int userId)
    {
        var userExp = await userExperienceRepository.GetByUserIdAsync(userId);
        return userExp?.StreakDays ?? 0;
    }
    
    public async Task<double> AddXpForStartTaskAsync(int userId)
    {
        var amount = GetXpAmountBySource(XpSourceType.StartTask);

        var userExp = await userExperienceRepository.GetByUserIdAsync(userId);

        if (userExp == null)
        {
            userExp = new UserExperience
            {
                UserId = userId,
                TotalXp = 0,
                StreakDays = 0,
                UpdatedAt = DateTime.UtcNow
            };
            await userExperienceRepository.CreateAsync(userExp);
        }

        userExp.TotalXp += amount;
        userExp.UpdatedAt = DateTime.UtcNow;
        userExperienceRepository.Update(userExp);

        await userXpLogRepository.CreateAsync(new UserXpLog
        {
            UserId = userId,
            XpAmount = amount,
            XpSource = XpSourceType.StartTask,
            CreatedAt = DateTime.UtcNow
        });

        await notificationService.PushNotificationAsync(userId, "XP Earned", $"You've earned {amount:F0} XP for starting a task!");

        await unitOfWork.CommitAsync();

        return amount;
    }
    
    private static double GetXpAmountBySource(XpSourceType source)
    {
        return source switch
        {
            XpSourceType.StartTask => 5,
            XpSourceType.DailyLogin => 10,
            _ => 0
        };
    }
}