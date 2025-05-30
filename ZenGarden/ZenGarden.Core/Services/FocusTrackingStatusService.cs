using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class FocusTrackingStatusService
{
    public static bool CanTransitionTo(FocusTrackingStatus currentStatus, FocusTrackingStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            // InProgress can transition to any state
            (FocusTrackingStatus.InProgress, _) => true,

            // Paused can only transition to InProgress or Abandoned
            (FocusTrackingStatus.Paused, FocusTrackingStatus.InProgress) => true,
            (FocusTrackingStatus.Paused, FocusTrackingStatus.Abandoned) => true,

            // Completed and Abandoned are terminal states
            (FocusTrackingStatus.Completed, _) => false,
            (FocusTrackingStatus.Abandoned, _) => false,

            // Default case
            _ => false
        };
    }

    public static bool IsTerminalState(FocusTrackingStatus status)
    {
        return status is FocusTrackingStatus.Completed or FocusTrackingStatus.Abandoned;
    }

    public static string GetTransitionMessage(FocusTrackingStatus currentStatus, FocusTrackingStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (FocusTrackingStatus.InProgress, FocusTrackingStatus.Paused) => "Focus session paused",
            (FocusTrackingStatus.InProgress, FocusTrackingStatus.Completed) => "Focus session completed",
            (FocusTrackingStatus.InProgress, FocusTrackingStatus.Abandoned) => "Focus session abandoned",
            (FocusTrackingStatus.Paused, FocusTrackingStatus.InProgress) => "Focus session resumed",
            (FocusTrackingStatus.Paused, FocusTrackingStatus.Abandoned) => "Focus session abandoned",
            _ => "Invalid status transition"
        };
    }

    public static bool CanBeResumed(FocusTrackingStatus status)
    {
        return status == FocusTrackingStatus.Paused;
    }
} 