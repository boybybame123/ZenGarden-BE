using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> CommitAsync();
    Task RollbackTransactionAsync();
    Task CommitTransactionAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    IExecutionStrategy CreateExecutionStrategy();
}