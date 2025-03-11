using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskTypesController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public TaskTypesController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/TaskTypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskType>>> GetTaskType()
    {
        return await _context.TaskType.ToListAsync();
    }

    // GET: api/TaskTypes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskType>> GetTaskType(int id)
    {
        var taskType = await _context.TaskType.FindAsync(id);

        if (taskType == null) return NotFound();

        return taskType;
    }

    // PUT: api/TaskTypes/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTaskType(int id, TaskType taskType)
    {
        if (id != taskType.TaskTypeId) return BadRequest();

        _context.Entry(taskType).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaskTypeExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/TaskTypes
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TaskType>> PostTaskType(TaskType taskType)
    {
        _context.TaskType.Add(taskType);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTaskType", new { id = taskType.TaskTypeId }, taskType);
    }

    // DELETE: api/TaskTypes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTaskType(int id)
    {
        var taskType = await _context.TaskType.FindAsync(id);
        if (taskType == null) return NotFound();

        _context.TaskType.Remove(taskType);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TaskTypeExists(int id)
    {
        return _context.TaskType.Any(e => e.TaskTypeId == id);
    }
}