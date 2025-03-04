using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class taskController : ControllerBase
{
    private readonly ITaskService taskService;

    public taskController(ITaskService taskService)
    {
        this.taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    // GET: api/task
    [HttpGet]
    public async Task<IActionResult> Gettasks()
    {
        var tasks = await taskService.GetAllUsersAsync();
        return Ok(tasks);
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GettaskById(int taskId)
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
    public async Task<IActionResult> Deletetask(int taskId)
    {
        await taskService.DeleteTaskAsync(taskId);
        return Ok(new { message = "task deleted successfully" });
    }

    [HttpPut("update-task")]
    [Produces("application/json")]
    public async Task<IActionResult> Updatetask(TaskDto task)
    {
        await taskService.UpdateTaskAsync(task);
        return Ok(new { message = "task updated successfully" });
    }
}