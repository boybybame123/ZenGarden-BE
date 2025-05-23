﻿using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UserXpLogService(
    IUserXpLogRepository userXpLogRepository,
    IUserExperienceRepository userExperienceRepository,
    INotificationService notificationService,
    IUserXpConfigRepository userXpConfigRepository,
    IUseItemService useItemService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
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
                LevelId = 1,
                IsMaxLevel = false,
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
        await CheckLevelUpAsync(userId);

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
                LevelId = 1,
                IsMaxLevel = false,
                StreakDays = 0,
                UpdatedAt = DateTime.UtcNow
            };
            await userExperienceRepository.CreateAsync(userExp);
        }

        userExp.TotalXp += amount;
        userExp.UpdatedAt = DateTime.UtcNow;
        userExperienceRepository.Update(userExp);
        await CheckLevelUpAsync(userId);

        await userXpLogRepository.CreateAsync(new UserXpLog
        {
            UserId = userId,
            XpAmount = amount,
            XpSource = XpSourceType.StartTask,
            CreatedAt = DateTime.UtcNow
        });

        await notificationService.PushNotificationAsync(userId, "XP Earned",
            $"You've earned {amount:F0} XP for starting a task!");

        await unitOfWork.CommitAsync();

        return amount;
    }

    public async Task CheckLevelUpAsync(int userId)
    {
        // Retrieve the user's experience data
        var userExp = await userExperienceRepository.GetByUserIdAsync(userId);
        if (userExp == null) return;

        // Fetch and sort all level configurations by XP threshold
        var allLevels = await userXpConfigRepository.GetAllAsync();
        var sortedLevels = allLevels.OrderBy(l => l.XpThreshold).ToList();

        var remainingXp = userExp.TotalXp;
        var currentLevel = userExp.LevelId;
        var hasLeveledUp = false;

        // Keep checking for level ups while user has enough XP
        while (true)
        {
            var currentLevelConfig = sortedLevels.FirstOrDefault(l => l.LevelId == currentLevel);
            if (currentLevelConfig == null) break; // Stop if current level config not found

            var nextLevelConfig = sortedLevels.FirstOrDefault(l => l.LevelId == currentLevel + 1);
            if (nextLevelConfig == null) break; // Stop if next level doesn't exist

            if (remainingXp < currentLevelConfig.XpThreshold)
                break;

            // Level up
            currentLevel++;
            remainingXp -= currentLevelConfig.XpThreshold;
            hasLeveledUp = true;

            // Notify the user of the level-up
            await notificationService.PushNotificationAsync(userId, "Level Up!",
                $"Congratulations! You've reached level {currentLevel}!");

            // Reward the user if the new level is a multiple of 5
            if (currentLevel % 5 == 0)
                await useItemService.GiftRandomItemFromListAsync(userId);
        }

        if (hasLeveledUp)
        {
            // Update the user's level and remaining XP
            userExp.LevelId = currentLevel;
            userExp.TotalXp = remainingXp;
            
            // Find the next level configuration
            var nextLevelConfig = sortedLevels.FirstOrDefault(l => l.LevelId == currentLevel + 1);
            
            // Update XP to next level and max-level status
            userExp.XpToNextLevel = nextLevelConfig != null
                ? (int)Math.Ceiling(nextLevelConfig.XpThreshold - remainingXp)
                : 0;
            userExp.IsMaxLevel = nextLevelConfig == null;

            // Save the updated user experience data
            userExperienceRepository.Update(userExp);
            await unitOfWork.CommitAsync();
        }
        else
        {
            // Update XP to next level and max-level status if no level-up occurred
            var nextLevelConfig = sortedLevels.FirstOrDefault(l => l.LevelId == userExp.LevelId + 1);
            var currentLevelConfig = sortedLevels.FirstOrDefault(l => l.LevelId == userExp.LevelId);
            
            userExp.XpToNextLevel = nextLevelConfig != null && currentLevelConfig != null
                ? (int)Math.Ceiling(nextLevelConfig.XpThreshold - userExp.TotalXp)
                : 0;
            userExp.IsMaxLevel = nextLevelConfig == null;

            userExperienceRepository.Update(userExp);
            await unitOfWork.CommitAsync();
        }
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

    public async Task<List<UserXpLogDto>> GetAllUserXpLogsAsync()
    {
        var logs = await userXpLogRepository.GetAllUserXpLogsAsync();
        return mapper.Map<List<UserXpLogDto>>(logs);
    }

    public async Task<List<UserXpLogDto>> GetUserXpLogsByUserIdAsync(int userId)
    {
        var logs = await userXpLogRepository.GetUserXpLogsByUserIdAsync(userId);
        return mapper.Map<List<UserXpLogDto>>(logs);
    }
}