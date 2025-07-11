using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.API.Middleware;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskController(
    ITaskService taskService,
    IValidator<UpdateTaskResultDto> updateTaskResultValidator)
    : ControllerBase
{
    private readonly ITaskService _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));

    private readonly IValidator<UpdateTaskResultDto> _updateTaskResultValidator =
        updateTaskResultValidator ?? throw new ArgumentNullException(nameof(updateTaskResultValidator));

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await _taskService.GetAllTaskAsync();
        return Ok(tasks);
    }

    [HttpGet("by-id/{taskId:int}")]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        if (taskId <= 0) return BadRequest(new { Message = "Invalid task ID" });

        var task = await _taskService.GetTaskByIdAsync(taskId);
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

    [HttpPatch("Update-Task/{taskId:int}")]
    public async Task<IActionResult> UpdateTask(int taskId, [FromForm] UpdateTaskDto task)
    {
        await _taskService.UpdateTaskAsync(taskId, task);
        return Ok(new { message = "Task updated successfully" });
    }

    [HttpPost("create-task")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        var createdTask = await _taskService.CreateTaskWithSuggestedMethodAsync(dto);
        return Ok(createdTask);
    }

    [HttpPost("create-multiple-tasks")]
    public async Task<IActionResult> CreateMultipleTasks([FromBody] List<CreateTaskDto> dtos)
    {
        var createdTasks = await _taskService.CreateMultipleTasksWithSuggestedMethodAsync(dtos);
        return Ok(createdTasks);
    }

    [Authorize]
    [HttpPost("start-task/{taskId:int}")]
    public async Task<IActionResult> StartTask(int taskId)
    {
        var userId = HttpContext.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        await _taskService.StartTaskAsync(taskId, userId.Value);
        return Ok(new { message = "Task started successfully." });
    }

    [HttpPost("complete-task/{taskId:int}")]
    public async Task<IActionResult> CompleteTask(int taskId, [FromForm] CompleteTaskDto completeTaskDto)
    {
        var xp = await _taskService.CompleteTaskAsync(taskId, completeTaskDto);
        return Ok(new
        {
            message = "Task completed successfully.",
            xpEarned = xp
        });
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

    [HttpPost("pause/{taskId:int}")]
    public async Task<IActionResult> PauseTask(int taskId)
    {
        await _taskService.PauseTaskAsync(taskId);
        return Ok(new { message = "Task paused successfully." });
    }

    [HttpPost("reorder/{userTreeId:int}")]
    public async Task<IActionResult> ReorderTasks(int userTreeId, [FromBody] List<ReorderTaskDto> reorderList,
        [FromServices] IValidator<List<ReorderTaskDto>> reorderValidator)
    {
        var validationResult = await reorderValidator.ValidateAsync(reorderList);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        await _taskService.ReorderTasksAsync(userTreeId, reorderList);
        return Ok(new { message = "reorder task successfully." });
    }

    [HttpPost("reset-weekly-priorities")]
    public async Task<IActionResult> ResetWeeklyTaskPriorities()
    {
        await _taskService.WeeklyTaskPriorityResetAsync();
        return Ok(new { message = "Weekly task priorities reset successfully." });
    }

    [HttpPut("force-update")]
    public async Task<IActionResult> ForceUpdateTaskStatus([FromBody] ForceUpdateTaskStatusDto dto)
    {
        await _taskService.ForceUpdateTaskStatusAsync(dto.TaskId, dto.NewStatus);
        return Ok(new { message = "Task status updated successfully." });
    }

    [HttpPut("{taskId:int}/task-type")]
    public async Task<IActionResult> UpdateTaskType(int taskId, [FromBody] UpdateTaskTypeIdDto dto,
        [FromServices] IValidator<UpdateTaskTypeIdDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        await _taskService.UpdateTaskTypeAsync(taskId, dto.NewTaskTypeId, dto.NewDuration);
        return NoContent();
    }

    [Authorize]
    [HttpPost("auto-pause")]
    public async Task<IActionResult> AutoPauseTasks()
    {
        await _taskService.AutoPauseTasksAsync();
        return Ok(new { message = "Auto pause executed successfully." });
    }

    [HttpPatch("{taskId:int}/duration")]
    public async Task<IActionResult> UpdateTaskDurationOnly(int taskId, [FromBody] UpdateTaskSimpleDto dto)
    {
        await _taskService.UpdateTaskSimpleAsync(taskId, dto);
        return Ok(new { message = "Task updated successfully" });
    }

    [HttpGet("user/{userId:int}/challenge/{challengeId:int}/cloned-tasks")]
    public async Task<IActionResult> GetClonedTasksByUserChallenge(int userId, int challengeId)
    {
        var tasks = await _taskService.GetClonedTasksByUserChallengeAsync(userId, challengeId);
        return Ok(tasks);
    }

    [HttpPut("{taskId:int}/result")]
    public async Task<IActionResult> UpdateTaskResult(
        int taskId,
        [FromForm] UpdateTaskResultDto updateTaskResultDto)
    {
        var validationResult = await _updateTaskResultValidator.ValidateAsync(updateTaskResultDto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        await _taskService.UpdateTaskResultAsync(taskId, updateTaskResultDto);
        return Ok(new { Message = "Task result updated successfully" });
    }

    [HttpGet("{taskId:int}/xp")]
    public async Task<ActionResult<TaskXpInfoDto>> GetTaskXpInfo(int taskId)
    { 
        var xpInfo = await _taskService.GetTaskXpInfoAsync(taskId); 
        return Ok(xpInfo);
    }

    [HttpGet("active/{userId:int}")]
    public async Task<ActionResult<TaskDto>> GetActiveTaskByUserId(int userId)
    {
        var task = await _taskService.GetActiveTaskByUserIdAsync(userId);
        if (task == null)
            return NotFound($"No active task found for user {userId}");

        return Ok(task);
    }
}