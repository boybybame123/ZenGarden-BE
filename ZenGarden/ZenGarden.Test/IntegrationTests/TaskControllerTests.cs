using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.Test.IntegrationTests;

public class TaskControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient(); // Khởi tạo HttpClient

    [Fact]
    public async Task GetTasks_ShouldReturnAllTasks()
    {
        // Act
        var response = await _client.GetAsync("/api/Task");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        Assert.NotNull(tasks);
        Assert.True(tasks.Count > 0); // Giả sử cơ sở dữ liệu có task
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnTask_WhenTaskExists()
    {
        // Arrange
        const int taskId = 1; // ID task giả định

        // Act
        var response = await _client.GetAsync($"/api/Task/by-id/{taskId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(task);
        Assert.Equal(taskId, task.TaskId);
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        const int taskId = 99999; // ID không tồn tại

        // Act
        var response = await _client.GetAsync($"/api/Task/by-id/{taskId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTasksByUserId_ShouldReturnTasks_WhenUserHasTasks()
    {
        // Arrange
        const int userId = 1;

        // Act
        var response = await _client.GetAsync($"/api/Task/by-user-id/{userId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        Assert.NotNull(tasks);
        Assert.True(tasks.Count > 0);
    }

    [Fact]
    public async Task GetTasksByUserTreeId_ShouldReturnTasks()
    {
        // Arrange
        const int userTreeId = 1;

        // Act
        var response = await _client.GetAsync($"/api/Task/by-user-tree/{userTreeId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        Assert.NotNull(tasks);
        Assert.True(tasks.Count > 0);
    }

    [Fact]
    public async Task DeleteTask_ShouldDeleteTaskSuccessfully()
    {
        // Arrange
        const int taskId = 1;

        // Act
        var response = await _client.DeleteAsync($"/api/Task/{taskId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Equal("task deleted successfully", result["message"]);
    }

    [Fact]
    public async Task UpdateTask_ShouldUpdateTaskSuccessfully()
    {
        // Arrange
        const int taskId = 1;
        var updateTaskDto = new UpdateTaskDto
        {
            TaskId = taskId,
            TaskName = "Updated Task",
            TaskDescription = "Updated description."
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Task/Update-Task/{taskId}", updateTaskDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Equal("Task updated successfully", result["message"]);
    }

    [Fact]
    public async Task CreateTask_ShouldReturnCreatedTask()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            TaskName = "New Task",
            TaskDescription = "New description",
            TaskTypeId = 1,
            UserTreeId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Task/create-task", createTaskDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(createdTask);
        Assert.Equal(createTaskDto.TaskName, createdTask.TaskName);
    }

    [Fact]
    public async Task StartTask_ShouldStartTaskSuccessfully()
    {
        // Arrange
        const int taskId = 1;

        // Act
        var response = await _client.PostAsync($"/api/Task/start-task/{taskId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Equal("Task started successfully.", result["message"]);
    }

    [Fact]
    public async Task CompleteTask_ShouldCompleteTaskSuccessfully()
    {
        // Arrange
        const int taskId = 1;

        // Act
        var response = await _client.PostAsync($"/api/Task/complete-task/{taskId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Equal("Task completed successfully.", result["message"]);
    }

    [Fact]
    public async Task UpdateOverdueTasks_ShouldUpdateOverdueTasksSuccessfully()
    {
        // Act
        var response = await _client.PostAsync("/api/Task/update-overdue", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Equal("Overdue tasks updated successfully.", result["message"]);
    }

    [Fact]
    public async Task CalculateTaskXp_ShouldReturnTaskXp()
    {
        // Arrange
        const int taskId = 1;

        // Act
        var response = await _client.GetAsync($"/api/Task/calculate-xp/{taskId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Equal(taskId, result["taskId"]);
        Assert.True(Convert.ToDouble(result["xpEarned"]) > 0);
    }

    [Fact]
    public async Task PauseTask_ShouldPauseTaskSuccessfully()
    {
        // Arrange
        const int taskId = 1;

        // Act
        var response = await _client.PutAsync($"/api/Task/pause/{taskId}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Equal("Task paused successfully.", result["message"]);
    }
}