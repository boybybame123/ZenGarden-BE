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
    public class UserXpConfigsController : ControllerBase
    {
        private readonly ZenGardenContext _context;

        public UserXpConfigsController(ZenGardenContext context)
        {
            _context = context;
        }

        // GET: api/UserXpConfigs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserXpConfig>>> GetUserXpConfig()
        {
            return await _context.UserXpConfig.ToListAsync();
        }

        // GET: api/UserXpConfigs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserXpConfig>> GetUserXpConfig(int id)
        {
            var userXpConfig = await _context.UserXpConfig.FindAsync(id);

            if (userXpConfig == null)
            {
                return NotFound();
            }

            return userXpConfig;
        }

        // PUT: api/UserXpConfigs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserXpConfig(int id, UserXpConfig userXpConfig)
        {
            if (id != userXpConfig.LevelId)
            {
                return BadRequest();
            }

            _context.Entry(userXpConfig).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserXpConfigExists(id))
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

        // POST: api/UserXpConfigs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserXpConfig>> PostUserXpConfig(UserXpConfig userXpConfig)
        {
            _context.UserXpConfig.Add(userXpConfig);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserXpConfig", new { id = userXpConfig.LevelId }, userXpConfig);
        }

        // DELETE: api/UserXpConfigs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserXpConfig(int id)
        {
            var userXpConfig = await _context.UserXpConfig.FindAsync(id);
            if (userXpConfig == null)
            {
                return NotFound();
            }

            _context.UserXpConfig.Remove(userXpConfig);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserXpConfigExists(int id)
        {
            return _context.UserXpConfig.Any(e => e.LevelId == id);
        }
    }
}
