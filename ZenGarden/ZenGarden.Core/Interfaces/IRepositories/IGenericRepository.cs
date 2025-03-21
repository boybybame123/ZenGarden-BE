using System.Linq.Expressions;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll(bool trackChanges = false);
    Task<List<T>> GetAllAsync(bool trackChanges = false);
    IQueryable<T> GetList(Expression<Func<T, bool>> expression, bool trackChanges = false);
    Task<T?> GetAsync(Expression<Func<T, bool>> expression, bool trackChanges = false);
    Task<T?> GetByIdAsync<TId>(TId id);
    Task<int> CountAsync(Expression<Func<T, bool>>? expression = null);

    Task CreateAsync(T entity);
    void Update(T entity);
    Task RemoveAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
}