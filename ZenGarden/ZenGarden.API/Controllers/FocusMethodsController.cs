using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FocusMethodsController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public FocusMethodsController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/FocusMethods
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FocusMethod>>> GetFocusMethod()
    {
        return await _context.FocusMethod.ToListAsync();
    }

    // GET: api/FocusMethods/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FocusMethod>> GetFocusMethod(int id)
    {
        var focusMethod = await _context.FocusMethod.FindAsync(id);

        if (focusMethod == null) return NotFound();

        return focusMethod;
    }

    // PUT: api/FocusMethods/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFocusMethod(int id, FocusMethod focusMethod)
    {
        if (id != focusMethod.FocusMethodId) return BadRequest();

        _context.Entry(focusMethod).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FocusMethodExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/FocusMethods
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<FocusMethod>> PostFocusMethod(FocusMethod focusMethod)
    {
        _context.FocusMethod.Add(focusMethod);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetFocusMethod", new { id = focusMethod.FocusMethodId }, focusMethod);
    }

    // DELETE: api/FocusMethods/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFocusMethod(int id)
    {
        var focusMethod = await _context.FocusMethod.FindAsync(id);
        if (focusMethod == null) return NotFound();

        _context.FocusMethod.Remove(focusMethod);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool FocusMethodExists(int id)
    {
        return _context.FocusMethod.Any(e => e.FocusMethodId == id);
    }
}