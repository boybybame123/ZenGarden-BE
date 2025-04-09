using System.Text;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
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
    private readonly Mock<IFocusMethodService> _focusMethodServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IS3Service> _s3ServiceMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly TaskService _taskService;
    private readonly Mock<ITaskTypeRepository> _taskTypeRepositoryMock;
    private readonly Mock<IValidator<CreateTaskDto>> _taskValidatorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserTreeRepository> _userTreeRepositoryMock;
    private readonly Mock<IXpConfigRepository> _xpConfigRepositoryMock;
    private readonly Mock<IXpConfigService> _xpConfigServiceMock;

    public TaskServiceTests()
    {
        _taskValidatorMock = new Mock<IValidator<CreateTaskDto>>();
        _userTreeRepositoryMock = new Mock<IUserTreeRepository>();
        _taskTypeRepositoryMock = new Mock<ITaskTypeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _mapperMock = new Mock<IMapper>();
        _focusMethodServiceMock = new Mock<IFocusMethodService>();
        _xpConfigServiceMock = new Mock<IXpConfigService>();
        _s3ServiceMock = new Mock<IS3Service>();
        _xpConfigRepositoryMock = new Mock<IXpConfigRepository>();

        _unitOfWorkMock
            .Setup(uow => uow.Repository<Tasks>())
            .Returns(_taskRepositoryMock.Object);

        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            Mock.Of<IFocusMethodRepository>(),
            _unitOfWorkMock.Object,
            _userTreeRepositoryMock.Object,
            _xpConfigRepositoryMock.Object,
            _taskTypeRepositoryMock.Object,
            Mock.Of<ITreeXpLogRepository>(),
            Mock.Of<IUserTreeService>(),
            _focusMethodServiceMock.Object,
            _xpConfigServiceMock.Object,
            Mock.Of<IUserChallengeService>(),
            Mock.Of<IChallengeTaskRepository>(),
            _s3ServiceMock.Object,
            _mapperMock.Object,
            Mock.Of<IBagRepository>(),
            Mock.Of<IBagItemRepository>(),
            Mock.Of<IUseItemService>(),
            _taskValidatorMock.Object
        );
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnTaskDto_WhenTaskExists()
    {
        // Arrange
        const int taskId = 1;
        var task = new Tasks { TaskId = taskId, Status = TasksStatus.NotStarted, TotalDuration = 60 };
        var expectedDto = new TaskDto { TaskId = taskId, RemainingTime = "60" };

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
        _unitOfWorkMock
            .Setup(uow => uow.CommitAsync())
            .ReturnsAsync(1);

        // Act
        await _taskService.DeleteTaskAsync(taskId);

        // Assert
        _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(taskId),
            Times.Once); // Kiểm tra GetByIdAsync được gọi đúng.
        _taskRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Tasks>()),
            Times.Once); // Kiểm tra RemoveAsync được gọi đúng.
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once); // Đảm bảo CommitAsync được gọi.
    }

    // [Fact]
    // public async Task UpdateTaskAsync_ShouldUpdateTaskSuccessfully()
    // {
    //     // Arrange
    //     var updateTaskDto = new UpdateTaskDto { TaskName = "Updated Task" };
    //     var taskEntity = new Tasks { TaskId = 1, TaskName = "Old Task", Status = TasksStatus.InProgress };
    //
    //     _taskRepositoryMock
    //         .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    //         .ReturnsAsync(taskEntity);
    //
    //     _mapperMock
    //         .Setup(mapper => mapper.Map(updateTaskDto, taskEntity))
    //         .Returns(taskEntity);
    //
    //     _unitOfWorkMock
    //         .Setup(uow => uow.CommitAsync())
    //         .ReturnsAsync(1);
    //
    //     // Act
    //     await _taskService.UpdateTaskAsync(updateTaskDto);
    //
    //     // Assert
    //     _taskRepositoryMock.Verify(repo => repo.Update(taskEntity), Times.Once);
    //     _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    // }

    [Fact]
    public async Task CreateTaskWithSuggestedMethodAsync_ShouldReturnCreatedTask()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            UserTreeId = 1,
            TaskName = "New Task",
            TaskTypeId = 1,
            TotalDuration = 30
        };

        var userTree = new UserTree
        {
            UserTreeId = createTaskDto.UserTreeId.Value,
            Name = "Test User Tree",
            User = new Users { UserId = 123 },
            TreeOwner = new Users { UserId = 123 },
            TreeXpConfig = new TreeXpConfig { LevelId = 1 }
        };

        _userTreeRepositoryMock
            .Setup(repo => repo.GetByIdAsync(createTaskDto.UserTreeId))
            .ReturnsAsync(userTree);

        var taskType = new TaskType { TaskTypeId = createTaskDto.TaskTypeId, TaskTypeName = "Focus" };
        _taskTypeRepositoryMock
            .Setup(repo => repo.GetByIdAsync(createTaskDto.TaskTypeId))
            .ReturnsAsync(taskType);

        var focusMethodDto = new FocusMethodDto { FocusMethodId = 1, DefaultDuration = 25, DefaultBreak = 5 };
        _focusMethodServiceMock
            .Setup(service => service.SuggestFocusMethodAsync(It.IsAny<SuggestFocusMethodDto>()))
            .ReturnsAsync(focusMethodDto);

        _mapperMock
            .Setup(mapper => mapper.Map<Tasks>(createTaskDto))
            .Returns(new Tasks
            {
                TaskId = 1,
                TaskName = createTaskDto.TaskName,
                UserTreeId = createTaskDto.UserTreeId,
                TaskTypeId = createTaskDto.TaskTypeId,
                FocusMethodId = 1
            });

        _mapperMock
            .Setup(mapper => mapper.Map<TaskDto>(It.IsAny<Tasks>()))
            .Returns(new TaskDto { TaskId = 1, TaskName = createTaskDto.TaskName });

        _xpConfigServiceMock
            .Setup(service => service.EnsureXpConfigExists(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _taskRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Tasks>()))
            .Returns(Task.CompletedTask);

        var mockTransaction = new Mock<IDbContextTransaction>();
        _unitOfWorkMock
            .Setup(uow => uow.BeginTransactionAsync())
            .ReturnsAsync(mockTransaction.Object);

        _taskValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskDto>(), CancellationToken.None))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _taskService.CreateTaskWithSuggestedMethodAsync(createTaskDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TaskId);
        Assert.Equal(createTaskDto.TaskName, result.TaskName);

        // Verify method calls
        _userTreeRepositoryMock.Verify(repo => repo.GetByIdAsync(createTaskDto.UserTreeId), Times.Once);
        _taskTypeRepositoryMock.Verify(repo => repo.GetByIdAsync(createTaskDto.TaskTypeId), Times.Once);
        _focusMethodServiceMock.Verify(service => service.SuggestFocusMethodAsync(It.IsAny<SuggestFocusMethodDto>()),
            Times.Once);
        _taskRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Tasks>()), Times.Once);
    }


    [Fact]
    public async Task AutoPauseTasksAsync_ShouldPauseEligibleTasks()
    {
        // Arrange
        var overdueTasks = new List<Tasks>
        {
            new() { TaskId = 1, Status = TasksStatus.InProgress, StartedAt = DateTime.UtcNow.AddHours(-3) },
            new() { TaskId = 2, Status = TasksStatus.InProgress, StartedAt = DateTime.UtcNow.AddHours(-5) }
        };
        _taskRepositoryMock
            .Setup(repo => repo.GetTasksInProgressBeforeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(overdueTasks);

        // Act
        await _taskService.AutoPauseTasksAsync();

        // Assert
        _taskRepositoryMock.Verify(
            repo => repo.Update(It.Is<Tasks>(t => t.TaskId == 1 && t.Status == TasksStatus.Paused)), Times.Once);
        _taskRepositoryMock.Verify(
            repo => repo.Update(It.Is<Tasks>(t => t.TaskId == 2 && t.Status == TasksStatus.Paused)), Times.Once);
    }

    [Fact]
    public async Task HandleTaskResultUpdate_ShouldHandleFileUpload()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileName = "example.txt";
        var content = "Test file content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.ContentType).Returns("text/plain");

        _s3ServiceMock
            .Setup(service => service.UploadFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync("https://example.com/uploaded/example.txt"); // Mock upload thành công

        // Act
        var result = await _taskService.HandleTaskResultUpdate(fileMock.Object, null);

        // Assert
        Assert.NotEmpty(result); // Thay vì NotNull(), kiểm tra kết quả không rỗng
        Assert.Equal("https://example.com/uploaded/example.txt", result); // Kiểm tra đúng URL mock
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
            UserTreeId = 1,
            TaskTypeId = 1
        };

        // ✅ Mock Validator để tránh bị NullReferenceException
        var validationResult = new ValidationResult(); // Không có lỗi
        _taskValidatorMock.Setup(v => v.ValidateAsync(validDto, CancellationToken.None))
            .ReturnsAsync(validationResult);

        // ✅ Mock `userTreeRepository`
        _userTreeRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new UserTree
            {
                UserTreeId = 1,
                Name = "Test User Tree",
                User = new Users { UserId = 123 },
                TreeOwner = new Users { UserId = 123 },
                TreeXpConfig = new TreeXpConfig { LevelId = 1 }
            });

        // ✅ Mock `taskTypeRepository`
        _taskTypeRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new TaskType { TaskTypeId = 1, TaskTypeName = "Focus Task" });

        // Act
        await _taskService.ValidateTaskDto(validDto);

        // Assert
        Assert.True(true); // Không có exception là thành công
    }


    [Fact]
    public async Task ValidateTaskDto_ShouldThrowException_WhenDtoIsInvalid()
    {
        // Arrange
        var dto = new CreateTaskDto();

        var validationFailures = new List<ValidationFailure>
        {
            new("TaskTypeId", "TaskTypeId is required"),
            new("UserTreeId", "UserTreeId is required")
        };

        _taskValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _taskService.ValidateTaskDto(dto));
    }

    [Fact]
    public async Task ValidateTaskDto_ShouldPass_WhenUserTreeIdIsNull()
    {
        // Arrange
        var validDto = new CreateTaskDto
        {
            TaskName = "Valid Task",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1),
            UserTreeId = null, // ✅ Test trường hợp không có UserTree
            TaskTypeId = 1
        };

        var validationResult = new ValidationResult(); // Không có lỗi
        _taskValidatorMock.Setup(v => v.ValidateAsync(validDto, CancellationToken.None))
            .ReturnsAsync(validationResult);

        _taskTypeRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new TaskType { TaskTypeId = 1, TaskTypeName = "Focus Task" });

        // Act & Assert
        await _taskService.ValidateTaskDto(validDto);
        Assert.True(true); // Thành công nếu không có lỗi
    }


    [Fact]
    public async Task CalculateTaskXp_ShouldReturnTaskXp()
    {
        // Arrange
        const int taskId = 1;

        // Mock the task repository to return a task
        _taskRepositoryMock
            .Setup(repo => repo.GetTaskWithDetailsAsync(taskId))
            .ReturnsAsync(new Tasks
            {
                TaskId = taskId,
                TaskTypeId = 1,
                FocusMethodId = 2
            });

        // Mock the XP config repository to return a configuration
        _xpConfigRepositoryMock
            .Setup(repo => repo.GetXpConfigAsync(1, 2))
            .ReturnsAsync(new XpConfig
            {
                TaskTypeId = 1,
                FocusMethodId = 2,
                BaseXp = 10,
                XpMultiplier = 1.5
            });

        // Act
        var xpResult = await _taskService.CalculateTaskXpAsync(taskId);

        // Assert
        Assert.True(xpResult > 0);
        Assert.Equal(15, xpResult); // 10 * 1.5 = 15
    }

    [Fact]
    public async Task AutoPauseTasksAsync_ShouldNotPause_WhenNoTasksAreEligible()
    {
        // Arrange
        var noOverdueTasks = new List<Tasks>();
        _taskRepositoryMock
            .Setup(repo => repo.GetTasksInProgressBeforeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(noOverdueTasks);

        // Act
        await _taskService.AutoPauseTasksAsync();

        // Assert
        _taskRepositoryMock.Verify(repo => repo.Update(It.IsAny<Tasks>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Never);
    }
}