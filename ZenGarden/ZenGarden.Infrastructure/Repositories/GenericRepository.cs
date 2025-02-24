using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class 
    {
        private readonly ZenGardenContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ZenGardenContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual IQueryable<T> GetAll(bool trackChanges = false) =>
            trackChanges ? _dbSet : _dbSet.AsNoTracking();

        public virtual async Task<List<T>> GetAllAsync(bool trackChanges = false) =>
            await (trackChanges ? _dbSet : _dbSet.AsNoTracking()).ToListAsync();

        public virtual IQueryable<T> GetList(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
            trackChanges ? _dbSet.Where(expression) : _dbSet.Where(expression).AsNoTracking();

        public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
            await (trackChanges ? _dbSet : _dbSet.AsNoTracking()).FirstOrDefaultAsync(expression);

        public virtual async Task<T?> GetByIdAsync<TId>(TId id) => await _dbSet.FindAsync(id);

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? expression = null) =>
            expression == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(expression);

        public virtual void Create(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Remove(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
        }
    }
}
