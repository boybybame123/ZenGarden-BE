namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> CommitAsync();
    Task RollbackTransactionAsync();
    Task CommitTransactionAsync();
    Task BeginTransactionAsync();
}