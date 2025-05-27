using System.Collections.Generic;
using System.Threading.Tasks;
using ZenGarden.Domain.DTOs.FocusTracking;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IFocusTrackingService
{
    Task<FocusTrackingDto> StartTrackingAsync(int userId, CreateFocusTrackingDto dto);
    Task<FocusTrackingDto> EndTrackingAsync(int trackingId, UpdateFocusTrackingDto dto);
    Task<FocusTrackingDto> GetTrackingByIdAsync(int trackingId);
    Task<List<FocusTrackingDto>> GetUserTrackingsAsync(int userId);
    Task<FocusActivityDto> AddActivityAsync(CreateFocusActivityDto dto);
    Task<List<FocusActivityDto>> GetTrackingActivitiesAsync(int trackingId);
    Task<double> CalculateXpEarnedAsync(int trackingId);
}