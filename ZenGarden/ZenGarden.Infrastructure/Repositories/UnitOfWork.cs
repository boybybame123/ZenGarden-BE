using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ZenGardenContext _context;

    public UnitOfWork(ZenGardenContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        return new GenericRepository<TEntity>(_context);
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
