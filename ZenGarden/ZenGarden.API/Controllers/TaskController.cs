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

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpDelete("{taskId}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        await _taskService.DeleteTaskAsync(taskId);
        return Ok(new { message = "task deleted successfully" });
    }

    [HttpPut("Update-Task")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateTask(TaskDto task)
    {
        await _taskService.UpdateTaskAsync(task);
        return Ok(new { message = "task updated successfully" });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        var task = await taskService.CreateTaskAsync(dto);
        return Ok(task);
    }
}