using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TaskRepository(ZenGardenContext context) : GenericRepository<Tasks>(context), ITaskRepository
{
    private readonly ZenGardenContext _context = context;


    
}

