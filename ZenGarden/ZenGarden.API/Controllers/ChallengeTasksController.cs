using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChallengeTasksController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public ChallengeTasksController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/ChallengeTasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChallengeTask>>> GetChallengeTask()
    {
        return await _context.ChallengeTask.ToListAsync();
    }

    // GET: api/ChallengeTasks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ChallengeTask>> GetChallengeTask(int id)
    {
        var challengeTask = await _context.ChallengeTask.FindAsync(id);

        if (challengeTask == null) return NotFound();

        return challengeTask;
    }

    // PUT: api/ChallengeTasks/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutChallengeTask(int id, ChallengeTask challengeTask)
    {
        if (id != challengeTask.ChallengeTaskId) return BadRequest();

        _context.Entry(challengeTask).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ChallengeTaskExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/ChallengeTasks
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<ChallengeTask>> PostChallengeTask(ChallengeTask challengeTask)
    {
        _context.ChallengeTask.Add(challengeTask);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetChallengeTask", new { id = challengeTask.ChallengeTaskId }, challengeTask);
    }

    // DELETE: api/ChallengeTasks/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChallengeTask(int id)
    {
        var challengeTask = await _context.ChallengeTask.FindAsync(id);
        if (challengeTask == null) return NotFound();

        _context.ChallengeTask.Remove(challengeTask);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ChallengeTaskExists(int id)
    {
        return _context.ChallengeTask.Any(e => e.ChallengeTaskId == id);
    }
}