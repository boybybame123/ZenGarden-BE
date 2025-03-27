using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserExperiencesController(ZenGardenContext context) : ControllerBase
{
    // GET: api/UserExperiences
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserExperience>>> GetUserExperience()
    {
        return await context.UserExperience.ToListAsync();
    }

    // GET: api/UserExperiences/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserExperience>> GetUserExperience(int id)
    {
        var userExperience = await context.UserExperience.FindAsync(id);

        if (userExperience == null) return NotFound();

        return userExperience;
    }

    // PUT: api/UserExperiences/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUserExperience(int id, UserExperience userExperience)
    {
        if (id != userExperience.UserExperienceId) return BadRequest();

        context.Entry(userExperience).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExperienceExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/UserExperiences
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<UserExperience>> PostUserExperience(UserExperience? userExperience)
    {
        // Check if userExperience is null and return a BadRequest
        if (userExperience == null)
        {
            return BadRequest(new { message = "UserExperience cannot be null." });
        }

        // Add the new user experience to the database
        context.UserExperience.Add(userExperience);
        await context.SaveChangesAsync();

        // Return the created user experience with a 201 Created status
        return CreatedAtAction("GetUserExperience", new { id = userExperience.UserExperienceId }, userExperience);
    }

    // DELETE: api/UserExperiences/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserExperience(int id)
    {
        var userExperience = await context.UserExperience.FindAsync(id);
        if (userExperience == null) return NotFound();

        context.UserExperience.Remove(userExperience);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExperienceExists(int id)
    {
        return context.UserExperience.Any(e => e.UserExperienceId == id);
    }
}