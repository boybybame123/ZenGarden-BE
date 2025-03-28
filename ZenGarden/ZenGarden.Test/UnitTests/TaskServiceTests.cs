using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Test.UnitTests;

public class TaskServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        // Mock các dependencies
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _mapperMock = new Mock<IMapper>();

        // Cấu hình UnitOfWork trả về repository
        _unitOfWorkMock
            .Setup(uow => uow.Repository<Tasks>())
            .Returns(_taskRepositoryMock.Object);

        // Khởi tạo service
        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            Mock.Of<IFocusMethodRepository>(),
            _unitOfWorkMock.Object,
            Mock.Of<IUserTreeRepository>(),
            Mock.Of<IXpConfigRepository>(),
            Mock.Of<ITaskTypeRepository>(),
            Mock.Of<ITreeXpLogRepository>(),
            Mock.Of<IUserTreeService>(),
            Mock.Of<IFocusMethodService>(),
            Mock.Of<IXpConfigService>(),
            Mock.Of<IUserChallengeRepository>(),
            Mock.Of<IChallengeTaskRepository>(),
            Mock.Of<IS3Service>(),
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnTaskDto_WhenTaskExists()
    {
        // Arrange
        const int taskId = 1;
        var task = new Tasks { TaskId = taskId, Status = TasksStatus.NotStarted, TotalDuration = 60 };
        var expectedDto = new TaskDto { TaskId = taskId, RemainingTime = 60 };

        _taskRepositoryMock
            .Setup(repo => repo.GetTaskWithDetailsAsync(taskId))
            .ReturnsAsync(task);

        _mapperMock
            .Setup(mapper => mapper.Map<TaskDto>(task))
            .Returns(expectedDto);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.TaskId, result.TaskId);
        Assert.Equal(expectedDto.RemainingTime, result.RemainingTime);

        _taskRepositoryMock.Verify(repo => repo.GetTaskWithDetailsAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldThrowKeyNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        const int taskId = 999;
        _taskRepositoryMock
            .Setup(repo => repo.GetTaskWithDetailsAsync(taskId))
            .ReturnsAsync((Tasks?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.GetTaskByIdAsync(taskId));
        _taskRepositoryMock.Verify(repo => repo.GetTaskWithDetailsAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldCallRepositoryRemoveAndCommit()
    {
        // Arrange
        const int taskId = 1;
        var taskEntity = new Tasks { TaskId = taskId, TaskName = "Test Task" };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(taskEntity); // Giả lập việc lấy task từ repo.

        _taskRepositoryMock
            .Setup(repo => repo.RemoveAsync(It.IsAny<Tasks>()))
            .Returns(Task.CompletedTask); // Giả lập việc xóa thành công.

        // Act
        await _taskService.DeleteTaskAsync(taskId);

        // Assert
        _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(taskId),
            Times.Once); // Kiểm tra GetByIdAsync được gọi đúng.
        _taskRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Tasks>()),
            Times.Once); // Kiểm tra RemoveAsync được gọi đúng.
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once); // Đảm bảo CommitAsync được gọi.
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldUpdateTaskSuccessfully()
    {
        // Arrange
        var updateTaskDto = new UpdateTaskDto { TaskId = 1, TaskName = "Updated Task" };
        var taskEntity = new Tasks { TaskId = 1, TaskName = "Old Task" };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(taskEntity);

        _mapperMock
            .Setup(mapper => mapper.Map(updateTaskDto, taskEntity))
            .Returns(taskEntity);

        // Act
        await _taskService.UpdateTaskAsync(updateTaskDto);

        // Assert
        _taskRepositoryMock.Verify(repo => repo.Update(taskEntity), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTaskWithSuggestedMethodAsync_ShouldReturnCreatedTask()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto { TaskName = "New Task" };
        var taskEntity = new Tasks { TaskId = 1, TaskName = "New Task" };
        var taskDto = new TaskDto { TaskId = 1, TaskName = "New Task" };

        _mapperMock
            .Setup(mapper => mapper.Map<Tasks>(createTaskDto))
            .Returns(taskEntity);

        _taskRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Tasks>()))
            .Callback<Tasks>(task =>
            {
                taskEntity.TaskId = task.TaskId; 
            });

        _mapperMock
            .Setup(mapper => mapper.Map<TaskDto>(taskEntity))
            .Returns(taskDto);

        // Act
        var result = await _taskService.CreateTaskWithSuggestedMethodAsync(createTaskDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskDto.TaskId, result.TaskId);
        Assert.Equal(taskDto.TaskName, result.TaskName);
        _taskRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Tasks>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task AutoPauseTasksAsync_ShouldPauseEligibleTasks()
    {
        // Arrange
        var overdueTasks = new List<Tasks>
        {
            new Tasks { TaskId = 1, Status = TasksStatus.InProgress, StartedAt = DateTime.UtcNow.AddHours(-3) },
            new Tasks { TaskId = 2, Status = TasksStatus.InProgress, StartedAt = DateTime.UtcNow.AddHours(-5) }
        };
        _taskRepositoryMock
            .Setup(repo => repo.GetTasksInProgressBeforeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(overdueTasks);

        // Act
        await _taskService.AutoPauseTasksAsync();

        // Assert
        _taskRepositoryMock.Verify(repo => repo.Update(It.Is<Tasks>(t => t.TaskId == 1 && t.Status == TasksStatus.Paused)), Times.Once);
        _taskRepositoryMock.Verify(repo => repo.Update(It.Is<Tasks>(t => t.TaskId == 2 && t.Status == TasksStatus.Paused)), Times.Once);
    }

    [Fact]
    public async Task HandleTaskResultUpdate_ShouldHandleFileUpload()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("example.txt");

        // Act
        var result = await _taskService.HandleTaskResultUpdate(mockFile.Object, null);

        // Assert
        Assert.NotNull(result); // Chỉnh sửa assertion phù hợp với logic của bạn
    }

    [Fact]
    public async Task HandleTaskResultUpdate_ShouldHandleUrlUpdate()
    {
        // Arrange
        var validUrl = "https://example.com/task-result";

        // Act
        var result = await _taskService.HandleTaskResultUpdate(null, validUrl);

        // Assert
        Assert.Equal(validUrl, result); // Chỉnh sửa assertion phù hợp với kết quả mong đợi
    }

    [Fact]
    public async Task ValidateTaskDto_ShouldPass_WhenDtoIsValid()
    {
        // Arrange
        var validDto = new CreateTaskDto
        {
            TaskName = "Valid Task",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1),
            UserTreeId = 1
        };

        // Act
        await _taskService.ValidateTaskDto(validDto);

        // Assert
        Assert.True(true); // Nếu không có exception, bài kiểm thử thành công
    }

    [Fact]
    public async Task ValidateTaskDto_ShouldThrowException_WhenDtoIsInvalid()
    {
        // Arrange
        var invalidDto = new CreateTaskDto
        {
            TaskName = string.Empty,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(-1),
            UserTreeId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _taskService.ValidateTaskDto(invalidDto));
    }

}