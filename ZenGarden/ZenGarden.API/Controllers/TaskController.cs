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

    [HttpPost("create-task")]
    public async Task<IActionResult> CreateTask([FromBody] FinalizeTaskDto dto)
    {
        var task = await _taskService.CreateTaskAsync(dto);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.TaskId }, task);
    }

    [HttpPost("suggest-focus-methods")]
    public async Task<IActionResult> GetSuggestedFocusMethods([FromBody] CreateTaskDto dto)
    {
        var result = await _taskService.GetSuggestedFocusMethodsAsync(dto.TaskName, dto.TaskDescription);

        return Ok(result);
    }

    [HttpPost("start-task/{taskId:int}")]
    public async Task<IActionResult> StartTask(int taskId)
    {
        await _taskService.StartTaskAsync(taskId);
        return Ok(new { message = "Task started successfully." });
    }

    [HttpPost("complete-task/{taskId:int}")]
    public async Task<IActionResult> CompleteTask(int taskId)
    {
        await _taskService.CompleteTaskAsync(taskId);
        return Ok(new { message = "Task completed successfully." });
    }
}