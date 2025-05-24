using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IFocusActivityRepository : IGenericRepository<FocusActivity>
{
    Task<List<FocusActivity>> GetByTrackingIdAsync(int trackingId);
    Task<int> GetTotalDistractionDurationAsync(int trackingId);
}