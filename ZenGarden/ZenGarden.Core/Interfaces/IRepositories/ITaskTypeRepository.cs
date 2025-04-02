using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface ITaskTypeRepository : IGenericRepository<TaskType>
{
    Task<int> GetTaskTypeIdByNameAsync(string taskTypeName);
}