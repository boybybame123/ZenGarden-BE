using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserChallengesController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public UserChallengesController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/UserChallenges
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserChallenge>>> GetUserChallenges()
    {
        return await _context.UserChallenges.ToListAsync();
    }

    // GET: api/UserChallenges/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserChallenge>> GetUserChallenge(int id)
    {
        var userChallenge = await _context.UserChallenges.FindAsync(id);

        if (userChallenge == null) return NotFound();

        return userChallenge;
    }

    // PUT: api/UserChallenges/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUserChallenge(int id, UserChallenge userChallenge)
    {
        if (id != userChallenge.UserChallengeId) return BadRequest();

        _context.Entry(userChallenge).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserChallengeExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/UserChallenges
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<UserChallenge>> PostUserChallenge(UserChallenge userChallenge)
    {
        _context.UserChallenges.Add(userChallenge);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUserChallenge", new { id = userChallenge.UserChallengeId }, userChallenge);
    }

    // DELETE: api/UserChallenges/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserChallenge(int id)
    {
        var userChallenge = await _context.UserChallenges.FindAsync(id);
        if (userChallenge == null) return NotFound();

        _context.UserChallenges.Remove(userChallenge);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserChallengeExists(int id)
    {
        return _context.UserChallenges.Any(e => e.UserChallengeId == id);
    }
}