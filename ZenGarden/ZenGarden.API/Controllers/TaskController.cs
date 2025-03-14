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

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        await _taskService.DeleteTaskAsync(taskId);
        return Ok(new { message = "task deleted successfully" });
    }

    [HttpPut("Update-Task")] 
    public async Task<IActionResult> UpdateTask(UpdateTaskDto task)
    {
        await _taskService.UpdateTaskAsync(task);
        return Ok(new { message = "task updated successfully" });
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
        if (HttpContext.Items["UserId"] is not int userId)
            return Unauthorized(new { message = "User authentication required." });

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

}