using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;
using ZenGarden.Infrastructure.Repositories;

namespace ZenGarden.Test.Mocks;

public class ChallengeTaskRepositoryTests
{
    [Fact]
    public async Task GetTasksByChallengeIdAsync_ShouldReturnCorrectTasks()
    {
        // Arrange: Tạo danh sách dữ liệu giả lập
        var challengeTasks = new List<ChallengeTask>
        {
            new()
            {
                ChallengeTaskId = 1, ChallengeId = 4, TaskId = 8,
                Tasks = new Tasks { TaskId = 8, TaskName = "Solve Array Problems" }
            },
            new()
            {
                ChallengeTaskId = 2, ChallengeId = 4, TaskId = 9,
                Tasks = new Tasks { TaskId = 9, TaskName = "Solve at least 3" }
            }
        }.AsQueryable();

        // Mock DbSet với hỗ trợ async
        var mockSet = new Mock<DbSet<ChallengeTask>>();
        mockSet.As<IQueryable<ChallengeTask>>().Setup(m => m.Provider).Returns(challengeTasks.Provider);
        mockSet.As<IQueryable<ChallengeTask>>().Setup(m => m.Expression).Returns(challengeTasks.Expression);
        mockSet.As<IQueryable<ChallengeTask>>().Setup(m => m.ElementType).Returns(challengeTasks.ElementType);
        mockSet.As<IQueryable<ChallengeTask>>().Setup(m => m.GetEnumerator()).Returns(challengeTasks.GetEnumerator());

        // Quan trọng: Mock hỗ trợ Async
        mockSet.As<IAsyncEnumerable<ChallengeTask>>()
            .Setup(m => m.GetAsyncEnumerator(CancellationToken.None))
            .Returns(new TestAsyncEnumerator<ChallengeTask>(challengeTasks.GetEnumerator()));

        mockSet.As<IQueryable<ChallengeTask>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<ChallengeTask>(challengeTasks.Provider));

        // Mock DbContext
        var mockContext = new Mock<ZenGardenContext>();
        mockContext.Setup(c => c.ChallengeTask).Returns(mockSet.Object);

        var repository = new ChallengeTaskRepository(mockContext.Object);

        // Act
        var result = await repository.GetTasksByChallengeIdAsync(4);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, ct => ct.Tasks is { TaskId: 8, TaskName: "Solve Array Problems" });
        Assert.Contains(result, ct => ct.Tasks is { TaskId: 9, TaskName: "Solve at least 3" });
    }
}

// Hỗ trợ async cho IQueryable
internal class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        return inner.Execute<TResult>(expression);
    }
}

// Hỗ trợ async cho IEnumerable
internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }
}

// Hỗ trợ async cho IEnumerator
internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        return default;
    }
}