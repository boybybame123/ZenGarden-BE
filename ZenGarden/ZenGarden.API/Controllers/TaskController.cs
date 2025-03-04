using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly ITaskService taskService;

    public TaskController(ITaskService taskService)
    {
        this.taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    // GET: api/task
    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await taskService.GetAllTaskAsync();
        return Ok(tasks);
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        var task = await taskService.GetTaskByIdAsync(taskId);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }
    [HttpDelete("{taskId}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        await taskService.DeleteTaskAsync(taskId);
        return Ok(new { message = "task deleted successfully" });
    }

    [HttpPut("Update-Task")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateTask(TaskDto task)
    {
        await taskService.UpdateTaskAsync(task);
        return Ok(new { message = "task updated successfully" });
    }
}