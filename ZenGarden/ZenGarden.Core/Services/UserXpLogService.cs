using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UserXpLogService(
    IUserXpLogRepository userXpLogRepository,
    IUserExperienceRepository userExperienceRepository,
    IUnitOfWork unitOfWork)
    : IUserXpLogService
{
    public async Task<List<DateTime>> GetUserCheckInHistoryAsync(int userId, int month, int year)
    {
        var checkIns = await userXpLogRepository.GetUserCheckInsByMonthAsync(userId, month, year);

        return checkIns.Select(log => log.CreatedAt.Date).Distinct().ToList();
    }


    public async Task<double> CheckInAndGetXpAsync(int userId)
    {
        const int xpBase = 10;
        const double streakBonusRate = 0.1;
        const int maxStreakDays = 7;

        var today = DateTime.UtcNow.Date;

        var existingCheckIn = await userXpLogRepository.GetUserCheckInLogAsync(userId, today);
        if (existingCheckIn != null)
            return 0;

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

        var consecutiveDays = userExp.StreakDays; // Số ngày check-in liên tiếp
        var streakDays = 0; // Mặc định chưa tính streak

        if (lastCheckIn != null)
        {
            var daysSinceLastCheckIn = (DateTime.UtcNow.Date - lastCheckIn.CreatedAt.Date).Days;

            consecutiveDays = daysSinceLastCheckIn switch
            {
                1 => consecutiveDays + 1, // Check-in liên tiếp -> +1 ngày
                > 1 => 0, // Bị gián đoạn -> reset về 0
                _ => consecutiveDays
            };

            // Khi đạt đủ 3 ngày liên tiếp thì streakDays bắt đầu tính là 1
            if (consecutiveDays >= 3)
                streakDays = Math.Min(consecutiveDays - 2, maxStreakDays); // Ngày thứ 3 => streak = 1
        }

        var streakMultiplier = 1 + (streakDays - 1) * streakBonusRate;
        var xpEarned = xpBase * streakMultiplier;

        userExp.TotalXp += xpEarned;
        userExp.StreakDays = consecutiveDays; // Lưu lại số ngày liên tiếp
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

        return xpEarned;
    }
    
    public async Task<int> GetCurrentStreakAsync(int userId)
    {
        var userExp = await userExperienceRepository.GetByUserIdAsync(userId);
        return userExp?.StreakDays ?? 0;
    }
}