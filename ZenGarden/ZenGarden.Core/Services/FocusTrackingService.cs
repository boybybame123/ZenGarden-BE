using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs.FocusTracking;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using AutoMapper;

namespace ZenGarden.Core.Services;

public class FocusTrackingService(
    IFocusTrackingRepository trackingRepository,
    IFocusActivityRepository activityRepository,
    IMapper mapper)
    : IFocusTrackingService
{
    public async Task<FocusTrackingDto> StartTrackingAsync(int userId, CreateFocusTrackingDto dto)
    {
        var tracking = mapper.Map<FocusTracking>(dto);
        tracking.UserId = userId;

        await trackingRepository.CreateAsync(tracking);
        return await GetTrackingByIdAsync(tracking.TrackingId);
    }

    public async Task<FocusTrackingDto> EndTrackingAsync(int trackingId, UpdateFocusTrackingDto dto)
    {
        var tracking = await trackingRepository.GetByIdAsync(trackingId);
        if (tracking == null)
            throw new KeyNotFoundException($"Tracking with ID {trackingId} not found");

        mapper.Map(dto, tracking);
        trackingRepository.Update(tracking);
        return await GetTrackingByIdAsync(trackingId);
    }

    public async Task<FocusTrackingDto> GetTrackingByIdAsync(int trackingId)
    {
        var tracking = await trackingRepository.GetByIdWithDetailsAsync(trackingId);
        if (tracking == null)
            throw new KeyNotFoundException($"Tracking with ID {trackingId} not found");

        return mapper.Map<FocusTrackingDto>(tracking);
    }

    public async Task<List<FocusTrackingDto>> GetUserTrackingsAsync(int userId)
    {
        var trackings = await trackingRepository.GetByUserIdAsync(userId);
        return mapper.Map<List<FocusTrackingDto>>(trackings);
    }

    public async Task<FocusActivityDto> AddActivityAsync(CreateFocusActivityDto dto)
    {
        var activity = mapper.Map<FocusActivity>(dto);
        await activityRepository.CreateAsync(activity);
        return mapper.Map<FocusActivityDto>(activity);
    }

    public async Task<List<FocusActivityDto>> GetTrackingActivitiesAsync(int trackingId)
    {
        var activities = await activityRepository.GetByTrackingIdAsync(trackingId);
        return mapper.Map<List<FocusActivityDto>>(activities);
    }

    public async Task<double> CalculateXpEarnedAsync(int trackingId)
    {
        var tracking = await trackingRepository.GetByIdWithDetailsAsync(trackingId);
        if (tracking == null)
            throw new KeyNotFoundException($"Tracking with ID {trackingId} not found");

        // Base XP from task type
        var baseXp = tracking.Task?.TaskType?.XpMultiplier ?? 1.0;

        // Focus method multiplier
        var focusMultiplier = tracking.FocusMethod?.XpMultiplier ?? 1.0;

        // Calculate actual duration in minutes
        var actualDuration = tracking.ActualDuration ?? 0;

        // Calculate distraction penalty
        var distractions = await activityRepository.GetTotalDistractionDurationAsync(trackingId);
        var distractionPenalty = distractions > 0 ? 0.8 : 1.0; // 20% penalty if there are distractions

        // Calculate final XP
        var xpEarned = baseXp * focusMultiplier * (actualDuration / 60.0) * distractionPenalty;

        return Math.Round(xpEarned, 2);
    }
}