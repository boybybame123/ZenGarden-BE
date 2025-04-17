using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemDetailsController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public ItemDetailsController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/ItemDetails
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDetail>>> GetItemDetail()
    {
        return await _context.ItemDetail.ToListAsync();
    }

    // GET: api/ItemDetails/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDetail>> GetItemDetail(int id)
    {
        var itemDetail = await _context.ItemDetail.FindAsync(id);

        if (itemDetail == null) return NotFound();

        return itemDetail;
    }

    // PUT: api/ItemDetails/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutItemDetail(int id, ItemDetail itemDetail)
    {
        if (id != itemDetail.ItemDetailId) return BadRequest();

        _context.Entry(itemDetail).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ItemDetailExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/ItemDetails
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<ItemDetail>> PostItemDetail(ItemDetail itemDetail)
    {
        _context.ItemDetail.Add(itemDetail);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetItemDetail", new { id = itemDetail.ItemDetailId }, itemDetail);
    }

    // DELETE: api/ItemDetails/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItemDetail(int id)
    {
        var itemDetail = await _context.ItemDetail.FindAsync(id);
        if (itemDetail == null) return NotFound();

        _context.ItemDetail.Remove(itemDetail);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ItemDetailExists(int id)
    {
        return _context.ItemDetail.Any(e => e.ItemDetailId == id);
    }
}