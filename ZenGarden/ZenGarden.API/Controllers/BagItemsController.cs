using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BagItemsController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public BagItemsController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/BagItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BagItem>>> GetBagItem()
    {
        return await _context.BagItem.ToListAsync();
    }

    // GET: api/BagItems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BagItem>> GetBagItem(int id)
    {
        var bagItem = await _context.BagItem.FindAsync(id);

        if (bagItem == null) return NotFound();

        return bagItem;
    }

    // PUT: api/BagItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBagItem(int id, BagItem bagItem)
    {
        if (id != bagItem.BagItemId) return BadRequest();

        _context.Entry(bagItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BagItemExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/BagItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<BagItem>> PostBagItem(BagItem bagItem)
    {
        _context.BagItem.Add(bagItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetBagItem", new { id = bagItem.BagItemId }, bagItem);
    }

    // DELETE: api/BagItems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBagItem(int id)
    {
        var bagItem = await _context.BagItem.FindAsync(id);
        if (bagItem == null) return NotFound();

        _context.BagItem.Remove(bagItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BagItemExists(int id)
    {
        return _context.BagItem.Any(e => e.BagItemId == id);
    }
}