using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IFocusTrackingRepository : IGenericRepository<FocusTracking>
{
    Task<List<FocusTracking>> GetByUserIdAsync(int userId);
    Task<FocusTracking?> GetByIdWithDetailsAsync(int id);
}