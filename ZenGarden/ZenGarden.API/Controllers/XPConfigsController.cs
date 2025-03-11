using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class XPConfigsController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public XPConfigsController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/XPConfigs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<XPConfig>>> GetXpConfigs()
    {
        return await _context.XpConfigs.ToListAsync();
    }

    // GET: api/XPConfigs/5
    [HttpGet("{id}")]
    public async Task<ActionResult<XPConfig>> GetXPConfig(int id)
    {
        var xPConfig = await _context.XpConfigs.FindAsync(id);

        if (xPConfig == null) return NotFound();

        return xPConfig;
    }

    // PUT: api/XPConfigs/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutXPConfig(int id, XPConfig xPConfig)
    {
        if (id != xPConfig.XPConfigId) return BadRequest();

        _context.Entry(xPConfig).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!XPConfigExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/XPConfigs
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<XPConfig>> PostXPConfig(XPConfig xPConfig)
    {
        _context.XpConfigs.Add(xPConfig);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetXPConfig", new { id = xPConfig.XPConfigId }, xPConfig);
    }

    // DELETE: api/XPConfigs/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteXPConfig(int id)
    {
        var xPConfig = await _context.XpConfigs.FindAsync(id);
        if (xPConfig == null) return NotFound();

        _context.XpConfigs.Remove(xPConfig);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool XPConfigExists(int id)
    {
        return _context.XpConfigs.Any(e => e.XPConfigId == id);
    }
}