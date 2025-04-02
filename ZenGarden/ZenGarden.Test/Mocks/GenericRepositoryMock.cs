using Moq;
using ZenGarden.Core.Interfaces.IRepositories;

namespace ZenGarden.Test.Mocks;

public static class GenericRepositoryMock<T> where T : class
{
    public static Mock<IGenericRepository<T>> GetMock(List<T>? initialData)
    {
        var mock = new Mock<IGenericRepository<T>>();
        var dataList = initialData ?? [];

        // Mock GetAllAsync
        mock.Setup(repo => repo.GetAllAsync(false))
            .ReturnsAsync(dataList);

        // Mock GetByIdAsync
        mock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => dataList.FirstOrDefault(e => (e as dynamic).Id == id));

        // Mock CreateAsync
        mock.Setup(repo => repo.CreateAsync(It.IsAny<T>()))
            .Returns((T entity) =>
            {
                dataList.Add(entity);
                return Task.CompletedTask;
            });

        // Mock Update
        mock.Setup(repo => repo.Update(It.IsAny<T>()))
            .Callback<T>(updatedEntity =>
            {
                var index = dataList.FindIndex(e => (e as dynamic).Id == (updatedEntity as dynamic).Id);
                if (index >= 0) dataList[index] = updatedEntity;
            });

        // Mock RemoveAsync
        mock.Setup(repo => repo.RemoveAsync(It.IsAny<T>()))
            .Returns((T entity) =>
            {
                dataList.Remove(entity);
                return Task.CompletedTask;
            });

        return mock;
    }
}