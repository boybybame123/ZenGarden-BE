using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class XpConfigRepository(ZenGardenContext context) : GenericRepository<XpConfig>(context), IXpConfigRepository
{
    private readonly ZenGardenContext _context1 = context;

    public async Task<XpConfig?> GetXpConfigAsync(int taskTypeId, int focusMethodId)
    {
        return await _context1.XpConfigs
            .FirstOrDefaultAsync(x => x.TaskTypeId == taskTypeId && x.FocusMethodId == focusMethodId);
    }
}