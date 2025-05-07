using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TreeXpLogsController : ControllerBase
{
    private readonly ITreeXpLogService _treeXpLogService;

    public TreeXpLogsController(ITreeXpLogService treeXpLogService)
    {
        _treeXpLogService = treeXpLogService;
    }

    // GET: api/TreeXpLogs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TreeXpLog>>> GetTreeXpLog()
    {
        var logs = await _treeXpLogService.GetAllTreeXpLog();
        return Ok(logs);
    }

    // GET: api/TreeXpLogs/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TreeXpLog>> GetTreeXpLog(int id)
    {
        var treeXpLog = await _treeXpLogService.GetTreeXpLogByLogIdAsync(id);

        if (treeXpLog == null) return NotFound();

        return Ok(treeXpLog);
    }

    // GET: api/TreeXpLogs/task/5
    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<IEnumerable<TreeXpLog>>> GetTreeXpLogByTaskId(int taskId)
    {
        var logs = await _treeXpLogService.GetTreeXpLogByTaskIdAsync(taskId);
        return Ok(logs);
    }

    // GET: api/TreeXpLogs/user-tree/latest/5
    [HttpGet("user-tree/latest/{userTreeId}")]
    public async Task<ActionResult<TreeXpLog>> GetLatestTreeXpLogByUserTreeId(int userTreeId)
    {
        var log = await _treeXpLogService.GetLatestTreeXpLogByUserTreeIdAsync(userTreeId);
        if (log == null) return NotFound();
        return Ok(log);
    }

    // GET: api/TreeXpLogs/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<TreeXpLog>>> GetTreeXpLogByUserId(int userId)
    {
        var logs = await _treeXpLogService.GetTreeXpLogByUserIdAsync(userId);
        return Ok(logs);
    }

    // GET: api/TreeXpLogs/user-tree/5
    [HttpGet("user-tree/{userTreeId}")]
    public async Task<ActionResult<IEnumerable<TreeXpLog>>> GetTreeXpLogByUserTreeId(int userTreeId)
    {
        var logs = await _treeXpLogService.GetTreeXpLogByUserTreeIdAsync(userTreeId);
        return Ok(logs);
    }
}