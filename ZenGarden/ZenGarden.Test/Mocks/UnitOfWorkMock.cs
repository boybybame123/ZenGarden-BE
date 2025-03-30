using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using ZenGarden.Core.Interfaces.IRepositories;

namespace ZenGarden.Test.Mocks;

public static class UnitOfWorkMock
{
    private static Mock<IUnitOfWork> GetMock()
    {
        var mock = new Mock<IUnitOfWork>();
        var transactionMock = new Mock<IDbContextTransaction>();

        // Mock transaction methods
        mock.Setup(uow => uow.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
        mock.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);
        mock.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        // Mock CommitAsync
        mock.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

        return mock;
    }

    public static Mock<IUnitOfWork> GetMockWithRepositories<T>(List<T> dataList) where T : class
    {
        var mock = GetMock();
        var repositoryMock = GenericRepositoryMock<T>.GetMock(dataList);

        mock.Setup(uow => uow.Repository<T>()).Returns(repositoryMock.Object);

        return mock;
    }
}