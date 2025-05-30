using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UnitOfWork(ZenGardenContext context, IRedisService redisService) : IUnitOfWork
{
    private readonly ZenGardenContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IRedisService _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
    private IDbContextTransaction? _transaction;

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        return new GenericRepository<TEntity>(_context, _redisService);
    }

    public IExecutionStrategy CreateExecutionStrategy()
    {
        return _context.Database.CreateExecutionStrategy();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            return _transaction;
        }

        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction;
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> CommitAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}