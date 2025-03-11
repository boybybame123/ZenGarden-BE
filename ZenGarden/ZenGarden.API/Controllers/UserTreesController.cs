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
    public class UserTreesController : ControllerBase
    {
        private readonly ZenGardenContext _context;

        public UserTreesController(ZenGardenContext context)
        {
            _context = context;
        }

        // GET: api/UserTrees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTree>>> GetUserTree()
        {
            return await _context.UserTree.ToListAsync();
        }

        // GET: api/UserTrees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTree>> GetUserTree(int id)
        {
            var userTree = await _context.UserTree.FindAsync(id);

            if (userTree == null)
            {
                return NotFound();
            }

            return userTree;
        }

        // PUT: api/UserTrees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserTree(int id, UserTree userTree)
        {
            if (id != userTree.UserTreeId)
            {
                return BadRequest();
            }

            _context.Entry(userTree).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserTreeExists(id))
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

        // POST: api/UserTrees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserTree>> PostUserTree(UserTree userTree)
        {
            _context.UserTree.Add(userTree);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserTree", new { id = userTree.UserTreeId }, userTree);
        }

        // DELETE: api/UserTrees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserTree(int id)
        {
            var userTree = await _context.UserTree.FindAsync(id);
            if (userTree == null)
            {
                return NotFound();
            }

            _context.UserTree.Remove(userTree);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserTreeExists(int id)
        {
            return _context.UserTree.Any(e => e.UserTreeId == id);
        }
    }
}
