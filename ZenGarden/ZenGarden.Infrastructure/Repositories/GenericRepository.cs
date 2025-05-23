using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ZenGardenContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly IRedisService _redisService;
    private const int DefaultCacheMinutes = 30;

    public GenericRepository(ZenGardenContext context, IRedisService redisService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
        _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
    }

    public virtual IQueryable<T> GetAll(bool trackChanges = false)
    {
        return trackChanges ? _dbSet : _dbSet.AsNoTracking();
    }

    public virtual async Task<List<T>> GetAllAsync(bool trackChanges = false)
    {
        string cacheKey = $"{typeof(T).Name}:all";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await (trackChanges ? _dbSet : _dbSet.AsNoTracking()).ToListAsync(),
            TimeSpan.FromMinutes(DefaultCacheMinutes)
        );
    }

    public virtual IQueryable<T> GetList(Expression<Func<T, bool>> expression, bool trackChanges = false)
    {
        return trackChanges ? _dbSet.Where(expression) : _dbSet.Where(expression).AsNoTracking();
    }

    public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> expression, bool trackChanges = false)
    {
        string cacheKey = $"{typeof(T).Name}:filter:{expression}";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await (trackChanges ? _dbSet.Where(expression) : _dbSet.Where(expression).AsNoTracking()).ToListAsync(),
            TimeSpan.FromMinutes(DefaultCacheMinutes)
        );
    }

    public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> expression, bool trackChanges = false)
    {
        string cacheKey = $"{typeof(T).Name}:single:{expression}";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await (trackChanges ? _dbSet : _dbSet.AsNoTracking()).FirstOrDefaultAsync(expression),
            TimeSpan.FromMinutes(DefaultCacheMinutes)
        );
    }

    public virtual async Task<T?> GetByIdAsync<TId>(TId id)
    {
        var cacheKey = $"{typeof(T).Name}:{id}";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await _dbSet.FindAsync(id),
            TimeSpan.FromMinutes(DefaultCacheMinutes)
        );
    }

    public virtual async Task<T?> GetWithRelationsAsync<TId>(TId id, params Expression<Func<T, object>>[] includes)
    {
        string cacheKey = $"{typeof(T).Name}:{id}:with_relations";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => {
                var query = _dbSet.AsQueryable();
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                return await query.FirstOrDefaultAsync(e => EF.Property<TId>(e, "Id")!.Equals(id));
            },
            TimeSpan.FromMinutes(DefaultCacheMinutes)
        );
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? expression = null)
    {
        string cacheKey = $"{typeof(T).Name}:count:{expression?.ToString() ?? "all"}";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => expression == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(expression),
            TimeSpan.FromMinutes(DefaultCacheMinutes)
        );
    }

    public virtual async Task CreateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity);
        await InvalidateCacheAsync();
    }

    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Entry(entity).State = EntityState.Modified;
        InvalidateCacheAsync().Wait();
    }

    public virtual async Task RemoveAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Task.Run(() => _dbSet.Remove(entity));
        await InvalidateCacheAsync();
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        var entityList = entities.ToList();
        if (entityList.Count == 0) return;

        await _dbSet.AddRangeAsync(entityList);
        await InvalidateCacheAsync();
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        var entityList = entities.ToList();
        if (entityList.Count == 0) return;

        _dbSet.UpdateRange(entityList);
        await InvalidateCacheAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        var cacheKey = $"{typeof(T).Name}:exists:{predicate}";
        return await _redisService.GetOrSetAsync(
            cacheKey,
            async () => await _dbSet.AnyAsync(predicate),
            TimeSpan.FromMinutes(DefaultCacheMinutes)
        );
    }

    private async Task InvalidateCacheAsync()
    {
        await _redisService.RemoveByPatternAsync($"{typeof(T).Name}:*");
    }
}