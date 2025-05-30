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
    ITaskRepository taskRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork)
    : IFocusTrackingService
{
    public async Task<FocusTrackingDto> StartTrackingAsync(int userId, CreateFocusTrackingDto dto)
    {
        // Validate task and focus method
        if (dto.TaskId <= 0)
            throw new ArgumentException("TaskId must be greater than 0");
        if (dto.FocusMethodId <= 0)
            throw new ArgumentException("FocusMethodId must be greater than 0");
        if (dto.PlannedDuration <= 0)
            throw new ArgumentException("PlannedDuration must be greater than 0");

        // Get and validate task
        var task = await taskRepository.GetByIdAsync(dto.TaskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {dto.TaskId} not found");

        var tracking = mapper.Map<FocusTracking>(dto);
        tracking.UserId = userId;
        tracking.Status = FocusTrackingStatus.InProgress;

        // Update task status
        task.StartedAt = DateTime.UtcNow;
        task.Status = TasksStatus.InProgress;
        taskRepository.Update(task);

        await trackingRepository.CreateAsync(tracking);
        await unitOfWork.CommitAsync();

        // Get the tracking with the generated ID
        var createdTracking = await trackingRepository.GetAsync(t => 
            t.UserId == userId && 
            t.StartTime == tracking.StartTime && 
            t.TaskId == tracking.TaskId
        );

        if (createdTracking == null)
            throw new Exception("Failed to create tracking");

        return await GetTrackingByIdAsync(createdTracking.TrackingId);
    }

    public async Task<FocusTrackingDto> EndTrackingAsync(int trackingId, UpdateFocusTrackingDto dto)
    {
        var tracking = await trackingRepository.GetByIdAsync(trackingId);
        if (tracking == null)
            throw new KeyNotFoundException($"Tracking with ID {trackingId} not found");

        // Validate status transition
        if (!FocusTrackingStatusService.CanTransitionTo(tracking.Status, dto.Status))
        {
            var message = FocusTrackingStatusService.GetTransitionMessage(tracking.Status, dto.Status);
            throw new ArgumentException(message);
        }

        // Get task
        var task = await taskRepository.GetByIdAsync(tracking.TaskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with ID {tracking.TaskId} not found");

        // If transitioning to a terminal state, update end time and calculate duration
        if (FocusTrackingStatusService.IsTerminalState(dto.Status))
        {
            tracking.EndTime = DateTime.UtcNow;
            tracking.ActualDuration = (int)(tracking.EndTime.Value - tracking.StartTime).TotalMinutes;

            // Update task
            task.CompletedAt = DateTime.UtcNow;
            task.Status = dto.Status == FocusTrackingStatus.Completed ? TasksStatus.Completed : TasksStatus.Canceled;
            task.AccumulatedTime = (task.AccumulatedTime ?? 0) + tracking.ActualDuration.Value;
            taskRepository.Update(task);

            // If tracking is completed, handle task completion
            if (dto.Status == FocusTrackingStatus.Completed)
            {
                await HandleTaskCompleteAsync(tracking.TaskId);
            }
        }

        mapper.Map(dto, tracking);
        trackingRepository.Update(tracking);
        await unitOfWork.CommitAsync();

        return await GetTrackingByIdAsync(trackingId);
    }

    public async Task<FocusTrackingDto> PauseTrackingAsync(int trackingId)
    {
        var tracking = await trackingRepository.GetByIdAsync(trackingId);
        if (tracking == null)
            throw new KeyNotFoundException($"Tracking with ID {trackingId} not found");

        if (!FocusTrackingStatusService.CanTransitionTo(tracking.Status, FocusTrackingStatus.Paused))
        {
            var message = FocusTrackingStatusService.GetTransitionMessage(tracking.Status, FocusTrackingStatus.Paused);
            throw new ArgumentException(message);
        }

        // Update task status
        var task = await taskRepository.GetByIdAsync(tracking.TaskId);
        if (task != null)
        {
            task.PausedAt = DateTime.UtcNow;
            task.Status = TasksStatus.Paused;
            taskRepository.Update(task);
        }

        tracking.Status = FocusTrackingStatus.Paused;
        trackingRepository.Update(tracking);
        await unitOfWork.CommitAsync();

        return await GetTrackingByIdAsync(trackingId);
    }

    public async Task<FocusTrackingDto> ResumeTrackingAsync(int trackingId)
    {
        var tracking = await trackingRepository.GetByIdAsync(trackingId);
        if (tracking == null)
            throw new KeyNotFoundException($"Tracking with ID {trackingId} not found");

        if (!FocusTrackingStatusService.CanBeResumed(tracking.Status))
        {
            var message = FocusTrackingStatusService.GetTransitionMessage(tracking.Status, FocusTrackingStatus.InProgress);
            throw new ArgumentException(message);
        }

        // Update task status
        var task = await taskRepository.GetByIdAsync(tracking.TaskId);
        if (task != null)
        {
            task.PausedAt = null;
            task.Status = TasksStatus.InProgress;
            taskRepository.Update(task);
        }

        tracking.Status = FocusTrackingStatus.InProgress;
        trackingRepository.Update(tracking);
        await unitOfWork.CommitAsync();

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
        // Check if tracking exists
        var tracking = await trackingRepository.GetByIdAsync(dto.TrackingId);
        if (tracking == null)
            throw new KeyNotFoundException($"Tracking with ID {dto.TrackingId} not found");

        var activity = mapper.Map<FocusActivity>(dto);
        await activityRepository.CreateAsync(activity);

        // Handle different activity types
        switch (activity.Type)
        {
            case FocusActivityType.Break:
                // Update task's break time
                var task = await taskRepository.GetByIdAsync(tracking.TaskId);
                if (task != null)
                {
                    task.BreakTime = (task.BreakTime ?? 0) + activity.Duration;
                    taskRepository.Update(task);
                }
                break;

            case FocusActivityType.TabSwitch:
            case FocusActivityType.TabOpen:
            case FocusActivityType.TabClose:
            case FocusActivityType.WindowOpen:
            case FocusActivityType.WindowClose:
            case FocusActivityType.WindowMinimize:
            case FocusActivityType.WindowMaximize:
            case FocusActivityType.BrowserOpen:
            case FocusActivityType.BrowserClose:
                // These activities are considered distractions
                activity.IsDistraction = true;
                // You might want to add logic here to:
                // 1. Count consecutive distractions
                // 2. Calculate distraction duration
                // 3. Update tracking statistics
                break;

            case FocusActivityType.ScreenLock:
            case FocusActivityType.SystemSleep:
                // These activities might indicate the user is away
                // You might want to:
                // 1. Pause the tracking
                // 2. Add a system activity
                await HandleTaskPauseAsync(tracking.TaskId);
                break;

            case FocusActivityType.ScreenUnlock:
            case FocusActivityType.SystemWake:
                // These activities might indicate the user is back
                // You might want to:
                // 1. Resume the tracking if it was paused
                // 2. Add a system activity
                await HandleTaskResumeAsync(tracking.TaskId);
                break;
        }

        await unitOfWork.CommitAsync();
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

    private async Task HandleTaskPauseAsync(int taskId)
    {
        // Find active tracking for this task
        var activeTracking = await trackingRepository.GetAsync(t => 
            t.TaskId == taskId && 
            t.Status == FocusTrackingStatus.InProgress
        );

        if (activeTracking != null)
        {
            // Pause the tracking
            activeTracking.Status = FocusTrackingStatus.Paused;
            trackingRepository.Update(activeTracking);

            // Add a system activity to record the pause
            var pauseActivity = new FocusActivity
            {
                TrackingId = activeTracking.TrackingId,
                Type = FocusActivityType.System,
                ActivityDetails = "Task was paused",
                Duration = 0,
                CreatedAt = DateTime.UtcNow
            };
            await activityRepository.CreateAsync(pauseActivity);

            await unitOfWork.CommitAsync();
        }
    }

    private async Task HandleTaskResumeAsync(int taskId)
    {
        // Find paused tracking for this task
        var pausedTracking = await trackingRepository.GetAsync(t => 
            t.TaskId == taskId && 
            t.Status == FocusTrackingStatus.Paused
        );

        if (pausedTracking != null)
        {
            // Resume the tracking
            pausedTracking.Status = FocusTrackingStatus.InProgress;
            trackingRepository.Update(pausedTracking);

            // Add a system activity to record the resume
            var resumeActivity = new FocusActivity
            {
                TrackingId = pausedTracking.TrackingId,
                Type = FocusActivityType.System,
                ActivityDetails = "Task was resumed",
                Duration = 0,
                CreatedAt = DateTime.UtcNow
            };
            await activityRepository.CreateAsync(resumeActivity);

            await unitOfWork.CommitAsync();
        }
    }

    private async Task HandleTaskCompleteAsync(int taskId)
    {
        // Find active or paused tracking for this task
        var tracking = await trackingRepository.GetAsync(t => 
            t.TaskId == taskId && 
            (t.Status == FocusTrackingStatus.InProgress || t.Status == FocusTrackingStatus.Paused)
        );

        if (tracking != null)
        {
            // Complete the tracking
            tracking.Status = FocusTrackingStatus.Completed;
            tracking.EndTime = DateTime.UtcNow;
            tracking.ActualDuration = (int)(tracking.EndTime.Value - tracking.StartTime).TotalMinutes;
            trackingRepository.Update(tracking);

            // Add a system activity to record the completion
            var completeActivity = new FocusActivity
            {
                TrackingId = tracking.TrackingId,
                Type = FocusActivityType.System,
                ActivityDetails = "Task was completed",
                Duration = 0,
                CreatedAt = DateTime.UtcNow
            };
            await activityRepository.CreateAsync(completeActivity);

            await unitOfWork.CommitAsync();
        }
    }
}