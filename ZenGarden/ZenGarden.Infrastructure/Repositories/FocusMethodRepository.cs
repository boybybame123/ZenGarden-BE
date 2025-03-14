using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class FocusMethodRepository(ZenGardenContext context) : GenericRepository<FocusMethod>(context), IFocusMethodRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<string>> GetMethodNamesAsync()
    {
        return await _context.FocusMethod
            .Where(fm => fm.IsActive)
            .Select(fm => fm.Name)
            .ToListAsync();
    }

    public async Task<FocusMethod?> GetByNameAsync(string name)
    {
        return await _context.FocusMethod
            .Where(fm => fm.Name == name)
            .FirstOrDefaultAsync();
    }
}