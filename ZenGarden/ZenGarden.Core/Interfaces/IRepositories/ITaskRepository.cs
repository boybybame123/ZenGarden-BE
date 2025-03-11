using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITaskRepository : IGenericRepository<Tasks>
{
    //public Task<Tasks?> GetUserTaskInProgressAsync(int userId);
}