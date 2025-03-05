using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class TaskFocusRepository(ZenGardenContext context)
    : GenericRepository<TaskFocusSetting>(context), ITaskFocusRepository;