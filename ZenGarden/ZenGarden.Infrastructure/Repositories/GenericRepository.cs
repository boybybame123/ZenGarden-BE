using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ZenGardenContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ZenGardenContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public virtual IQueryable<T> GetAll(bool trackChanges = false)
    {
        return trackChanges ? _dbSet : _dbSet.AsNoTracking();
    }

    public virtual async Task<List<T>> GetAllAsync(bool trackChanges = false)
    {
        return await (trackChanges ? _dbSet : _dbSet.AsNoTracking()).ToListAsync();
    }

    public virtual IQueryable<T> GetList(Expression<Func<T, bool>> expression, bool trackChanges = false)
    {
        return trackChanges ? _dbSet.Where(expression) : _dbSet.Where(expression).AsNoTracking();
    }

    public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> expression, bool trackChanges = false)
    {
        return await (trackChanges ? _dbSet : _dbSet.AsNoTracking()).FirstOrDefaultAsync(expression);
    }

    public virtual async Task<T?> GetByIdAsync<TId>(TId id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? expression = null)
    {
        return expression == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(expression);
    }

    public virtual async Task CreateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public virtual async Task RemoveAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Task.Run(() => _dbSet.Remove(entity));
    }
}
