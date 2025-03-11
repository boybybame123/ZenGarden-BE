using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TreeXpLogsController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public TreeXpLogsController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/TreeXpLogs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TreeXpLog>>> GetTreeXpLog()
    {
        return await _context.TreeXpLog.ToListAsync();
    }

    // GET: api/TreeXpLogs/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TreeXpLog>> GetTreeXpLog(int id)
    {
        var treeXpLog = await _context.TreeXpLog.FindAsync(id);

        if (treeXpLog == null) return NotFound();

        return treeXpLog;
    }

    // PUT: api/TreeXpLogs/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTreeXpLog(int id, TreeXpLog treeXpLog)
    {
        if (id != treeXpLog.LogId) return BadRequest();

        _context.Entry(treeXpLog).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TreeXpLogExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/TreeXpLogs
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TreeXpLog>> PostTreeXpLog(TreeXpLog treeXpLog)
    {
        _context.TreeXpLog.Add(treeXpLog);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTreeXpLog", new { id = treeXpLog.LogId }, treeXpLog);
    }

    // DELETE: api/TreeXpLogs/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTreeXpLog(int id)
    {
        var treeXpLog = await _context.TreeXpLog.FindAsync(id);
        if (treeXpLog == null) return NotFound();

        _context.TreeXpLog.Remove(treeXpLog);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TreeXpLogExists(int id)
    {
        return _context.TreeXpLog.Any(e => e.LogId == id);
    }
}