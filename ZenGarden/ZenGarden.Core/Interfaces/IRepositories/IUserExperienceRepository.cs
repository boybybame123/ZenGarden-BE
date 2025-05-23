using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserExperienceRepository : IGenericRepository<UserExperience>
{
    Task<UserExperience?> GetByUserIdAsync(int userId);
}