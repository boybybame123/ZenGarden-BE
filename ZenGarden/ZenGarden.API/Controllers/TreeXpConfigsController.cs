using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TreeXpConfigsController(ITreeXpLogService treeXpLogService) : ControllerBase
{
    [HttpGet("GetAllTreeXpLog")]
    public async Task<IActionResult> GetAllTreeXpLog()
    {
        var treeXpLogs = await treeXpLogService.GetAllTreeXpLog();
        return Ok(treeXpLogs);
    }

    [HttpGet("GetTreeXpLogById/{treeXpLogId:int}")]
    public async Task<IActionResult> GetTreeXpLogById(int treeXpLogId)
    {
        var treeXpLog = await treeXpLogService.GetTreeXpLogByLogIdAsync(treeXpLogId);
        if (treeXpLog == null) return NotFound();
        return Ok(treeXpLog);
    }

    [HttpGet("GetTreeXpLogByTaskId /{taskId:int}")]
    public async Task<IActionResult> GetTreeXpLogByTaskId(int taskId)
    {
        var treeXpLogs = await treeXpLogService.GetTreeXpLogByTaskIdAsync(taskId);
        return Ok(treeXpLogs);
    }
}