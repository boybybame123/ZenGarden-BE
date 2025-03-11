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
    public class TreesController : ControllerBase
    {
        private readonly ZenGardenContext _context;

        public TreesController(ZenGardenContext context)
        {
            _context = context;
        }

        // GET: api/Trees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tree>>> GetTree()
        {
            return await _context.Tree.ToListAsync();
        }

        // GET: api/Trees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tree>> GetTree(int id)
        {
            var tree = await _context.Tree.FindAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            return tree;
        }

        // PUT: api/Trees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTree(int id, Tree tree)
        {
            if (id != tree.TreeId)
            {
                return BadRequest();
            }

            _context.Entry(tree).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TreeExists(id))
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

        // POST: api/Trees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tree>> PostTree(Tree tree)
        {
            _context.Tree.Add(tree);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTree", new { id = tree.TreeId }, tree);
        }

        // DELETE: api/Trees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTree(int id)
        {
            var tree = await _context.Tree.FindAsync(id);
            if (tree == null)
            {
                return NotFound();
            }

            _context.Tree.Remove(tree);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TreeExists(int id)
        {
            return _context.Tree.Any(e => e.TreeId == id);
        }
    }
}
