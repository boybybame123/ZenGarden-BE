using Moq;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Test.Mocks;

public static class TaskRepositoryMock
{
    public static Mock<ITaskRepository> GetMock()
    {
        var mock = new Mock<ITaskRepository>();

        // Mock AddAsync (kế thừa từ GenericRepository)
        mock.Setup(repo => repo.CreateAsync(It.IsAny<Tasks>()))
            .Returns(Task.CompletedTask); // Giả lập việc thêm thành công.

        // Mock RemoveAsync (kế thừa từ GenericRepository)
        mock.Setup(repo => repo.RemoveAsync(It.IsAny<Tasks>()))
            .Returns(Task.CompletedTask); // Giả lập việc xoá thành công.

        // Mock UpdateAsync (kế thừa từ GenericRepository)
        mock.Setup(repo => repo.Update(It.IsAny<Tasks>()))
            .Callback<Tasks>(task =>
            {
                // Giả lập hành vi nếu cần, ví dụ lưu trạng thái của đối tượng.
                task.Status = TasksStatus.Completed;
            });

        // Mock GetByIdAsync (kế thừa từ GenericRepository)
        mock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) =>
                new Tasks
                {
                    TaskId = id,
                    Status = TasksStatus.Completed,
                    TotalDuration = 60
                });

        // Mock GetTaskWithDetailsAsync
        mock.Setup(repo => repo.GetTaskWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync((int taskId) =>
                taskId == 1 ? new Tasks { TaskId = taskId, TaskTypeId = 2, FocusMethodId = 3 } : null);


        // Mock GetAllWithDetailsAsync
        mock.Setup(repo => repo.GetAllWithDetailsAsync())
            .ReturnsAsync([
                new Tasks { TaskId = 1, Status = TasksStatus.Completed, TotalDuration = 60 },
                new Tasks { TaskId = 2, Status = TasksStatus.NotStarted, TotalDuration = 30 },
                new Tasks { TaskId = 3, Status = TasksStatus.InProgress, TotalDuration = 120 }
            ]);

        // Mock GetTasksByUserTreeIdAsync
        mock.Setup(repo => repo.GetTasksByUserTreeIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int treeId) =>
            [
                new Tasks { TaskId = 1, UserTreeId = treeId, Status = TasksStatus.InProgress },
                new Tasks { TaskId = 2, UserTreeId = treeId, Status = TasksStatus.Completed }
            ]);

        // Mock GetTasksByUserIdAsync
        mock.Setup(repo => repo.GetTasksByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int userId) =>
            [
                new Tasks
                {
                    TaskId = 1,
                    UserTree = new UserTree
                    {
                        UserId = userId,
                        Name = "Tree 1", // Giá trị cần thiết cho thuộc tính Name
                        TreeXpConfig = new TreeXpConfig(), // Giá trị cần thiết cho TreeXpConfig
                        User = new Users { UserId = userId, UserName = "User 1" }, // Giá trị cần thiết cho User
                        TreeOwner = new Users
                            { UserId = userId, UserName = "Owner 1" } // Giá trị cần thiết cho TreeOwner
                    },
                    Status = TasksStatus.Completed
                },

                new Tasks
                {
                    TaskId = 2,
                    UserTree = new UserTree
                    {
                        UserId = userId,
                        Name = "Tree 2", // Giá trị cần thiết
                        TreeXpConfig = new TreeXpConfig(), // Giá trị cần thiết cho TreeXpConfig
                        User = new Users { UserId = userId, UserName = "User 2" }, // Giá trị cần thiết cho User
                        TreeOwner = new Users
                            { UserId = userId, UserName = "Owner 2" } // Giá trị cần thiết cho TreeOwner
                    },
                    Status = TasksStatus.NotStarted
                }
            ]);

        // Mock GetOverdueTasksAsync
        mock.Setup(repo => repo.GetOverdueTasksAsync())
            .ReturnsAsync([
                new Tasks { TaskId = 1, Status = TasksStatus.InProgress, EndDate = DateTime.UtcNow.AddDays(-1) },
                new Tasks { TaskId = 2, Status = TasksStatus.InProgress, EndDate = DateTime.UtcNow.AddDays(-2) }
            ]);

        // Mock GetTasksInProgressBeforeAsync
        mock.Setup(repo => repo.GetTasksInProgressBeforeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((DateTime threshold) =>
            [
                new Tasks
                {
                    TaskId = 1,
                    Status = TasksStatus.InProgress,
                    StartedAt = threshold.AddMinutes(-10)
                },

                new Tasks
                {
                    TaskId = 2,
                    Status = TasksStatus.InProgress,
                    StartedAt = threshold.AddMinutes(-20)
                }
            ]);

        // Mock GetActiveTaskByUserTreeIdAsync
        mock.Setup(repo => repo.GetActiveTaskByUserTreeIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int treeId) =>
                new Tasks
                {
                    TaskId = 1,
                    UserTreeId = treeId,
                    Status = TasksStatus.InProgress,
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
                });

        return mock;
    }
}