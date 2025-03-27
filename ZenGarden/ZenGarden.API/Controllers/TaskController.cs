using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await _taskService.GetAllTaskAsync();
        return Ok(tasks);
    }

    [HttpGet("by-id/{taskId:int}")]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpGet("by-user-tree/{userTreeId:int}")]
    public async Task<IActionResult> GetTasksByUserTreeId(int userTreeId)
    {
        var task = await _taskService.GetTaskByUserTreeIdAsync(userTreeId);
        return Ok(task);
    }

    [HttpGet("by-user-id/{userId:int}")]
    public async Task<IActionResult> GetTasksByUserId(int userId)
    {
        var task = await _taskService.GetTaskByUserIdAsync(userId);
        return Ok(task);
    }

    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        await _taskService.DeleteTaskAsync(taskId);
        return Ok(new { message = "task deleted successfully" });
    }

    [HttpPut("Update-Task/{taskId:int}")]
    public async Task<IActionResult> UpdateTask(
        int taskId, [FromBody] UpdateTaskDto task)
    {
        if (taskId != task.TaskId)
            return BadRequest(new { message = "Task ID mismatch" });

        await _taskService.UpdateTaskAsync(task);
        return Ok(new { message = "Task updated successfully" });
    }


    [HttpPost("create-task")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        var createdTask = await _taskService.CreateTaskWithSuggestedMethodAsync(dto);
        return Ok(createdTask);
    }

    [Authorize]
    [HttpPost("start-task/{taskId:int}")]
    public async Task<IActionResult> StartTask(int taskId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId)) return Unauthorized();

        await _taskService.StartTaskAsync(taskId, userId);
        return Ok(new { message = "Task started successfully." });
    }

    [HttpPost("complete-task/{taskId:int}")]
    public async Task<IActionResult> CompleteTask(int taskId)
    {
        await _taskService.CompleteTaskAsync(taskId);
        return Ok(new { message = "Task completed successfully." });
    }

    [HttpPost("update-overdue")]
    public async Task<IActionResult> UpdateOverdueTasks()
    {
        await _taskService.UpdateOverdueTasksAsync();
        return Ok(new { message = "Overdue tasks updated successfully." });
    }

    [HttpGet("calculate-xp/{taskId:int}")]
    public async Task<IActionResult> CalculateTaskXp(int taskId)
    {
        var xpEarned = await _taskService.CalculateTaskXpAsync(taskId);
        return Ok(new { taskId, xpEarned });
    }

    [HttpPut("pause/{taskId:int}")]
    public async Task<IActionResult> PauseTask(int taskId)
    {
        await _taskService.PauseTaskAsync(taskId);
        return Ok(new { message = "Task paused successfully." });
    }
}