using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TaskTypeRepository(ZenGardenContext context) : GenericRepository<TaskType>(context), ITaskTypeRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<int> GetTaskTypeIdByNameAsync(string taskTypeName)
    {
        var taskType = await _context.TaskType
            .Where(t => t.TaskTypeName.ToLower() == taskTypeName.ToLower())
            .FirstOrDefaultAsync();

        if (taskType == null) throw new KeyNotFoundException($"TaskType with name '{taskTypeName}' not found.");

        return taskType.TaskTypeId;
    }
}