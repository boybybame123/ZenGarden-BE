using System.Linq.Expressions;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll(bool trackChanges = false);
    Task<List<T>> GetAllAsync(bool trackChanges = false);
    IQueryable<T> GetList(Expression<Func<T, bool>> expression, bool trackChanges = false);
    Task<List<T>> GetListAsync(Expression<Func<T, bool>> expression, bool trackChanges = false);
    Task<T?> GetAsync(Expression<Func<T, bool>> expression, bool trackChanges = false);
    Task<T?> GetByIdAsync<TId>(TId id);
    Task<T?> GetWithRelationsAsync<TId>(TId id, params Expression<Func<T, object>>[] includes);
    Task<int> CountAsync(Expression<Func<T, bool>>? expression = null);

    Task CreateAsync(T entity);

    void Update(T entity);


    Task RemoveAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}