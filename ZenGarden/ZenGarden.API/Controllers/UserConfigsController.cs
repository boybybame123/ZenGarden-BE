using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserConfigsController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public UserConfigsController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/UserConfigs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserConfig>>> GetUserConfig()
    {
        return await _context.UserConfig.ToListAsync();
    }

    // GET: api/UserConfigs/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserConfig>> GetUserConfig(int id)
    {
        var userConfig = await _context.UserConfig.FindAsync(id);

        if (userConfig == null) return NotFound();

        return userConfig;
    }

    // PUT: api/UserConfigs/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUserConfig(int id, UserConfig userConfig)
    {
        if (id != userConfig.UserConfigId) return BadRequest();

        _context.Entry(userConfig).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserConfigExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/UserConfigs
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<UserConfig>> PostUserConfig(UserConfig userConfig)
    {
        _context.UserConfig.Add(userConfig);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUserConfig", new { id = userConfig.UserConfigId }, userConfig);
    }

    // DELETE: api/UserConfigs/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserConfig(int id)
    {
        var userConfig = await _context.UserConfig.FindAsync(id);
        if (userConfig == null) return NotFound();

        _context.UserConfig.Remove(userConfig);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserConfigExists(int id)
    {
        return _context.UserConfig.Any(e => e.UserConfigId == id);
    }
}