using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TreeXpConfigsController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public TreeXpConfigsController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/TreeXpConfigs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TreeXpConfig>>> GetTreeXpConfig()
    {
        return await _context.TreeXpConfig.ToListAsync();
    }

    // GET: api/TreeXpConfigs/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TreeXpConfig>> GetTreeXpConfig(int id)
    {
        var treeXpConfig = await _context.TreeXpConfig.FindAsync(id);

        if (treeXpConfig == null) return NotFound();

        return treeXpConfig;
    }

    // PUT: api/TreeXpConfigs/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTreeXpConfig(int id, TreeXpConfig treeXpConfig)
    {
        if (id != treeXpConfig.LevelId) return BadRequest();

        _context.Entry(treeXpConfig).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TreeXpConfigExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/TreeXpConfigs
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TreeXpConfig>> PostTreeXpConfig(TreeXpConfig treeXpConfig)
    {
        _context.TreeXpConfig.Add(treeXpConfig);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTreeXpConfig", new { id = treeXpConfig.LevelId }, treeXpConfig);
    }

    // DELETE: api/TreeXpConfigs/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTreeXpConfig(int id)
    {
        var treeXpConfig = await _context.TreeXpConfig.FindAsync(id);
        if (treeXpConfig == null) return NotFound();

        _context.TreeXpConfig.Remove(treeXpConfig);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TreeXpConfigExists(int id)
    {
        return _context.TreeXpConfig.Any(e => e.LevelId == id);
    }
}