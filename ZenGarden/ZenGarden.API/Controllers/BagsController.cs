using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BagsController : ControllerBase
    {
        private readonly ZenGardenContext _context;

        public BagsController(ZenGardenContext context)
        {
            _context = context;
        }

        // GET: api/Bags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bag>>> GetBag()
        {
            return await _context.Bag.ToListAsync();
        }

        // GET: api/Bags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bag>> GetBag(int id)
        {
            var bag = await _context.Bag.FindAsync(id);

            if (bag == null)
            {
                return NotFound();
            }

            return bag;
        }

        // PUT: api/Bags/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBag(int id, Bag bag)
        {
            if (id != bag.BagId)
            {
                return BadRequest();
            }

            _context.Entry(bag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BagExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Bags
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Bag>> PostBag(Bag bag)
        {
            _context.Bag.Add(bag);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBag", new { id = bag.BagId }, bag);
        }

        // DELETE: api/Bags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBag(int id)
        {
            var bag = await _context.Bag.FindAsync(id);
            if (bag == null)
            {
                return NotFound();
            }

            _context.Bag.Remove(bag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BagExists(int id)
        {
            return _context.Bag.Any(e => e.BagId == id);
        }
    }
}
