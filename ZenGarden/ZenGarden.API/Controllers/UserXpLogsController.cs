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
    public class UserXpLogsController : ControllerBase
    {
        private readonly ZenGardenContext _context;

        public UserXpLogsController(ZenGardenContext context)
        {
            _context = context;
        }

        // GET: api/UserXpLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserXpLog>>> GetUserXpLog()
        {
            return await _context.UserXpLog.ToListAsync();
        }

        // GET: api/UserXpLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserXpLog>> GetUserXpLog(int id)
        {
            var userXpLog = await _context.UserXpLog.FindAsync(id);

            if (userXpLog == null)
            {
                return NotFound();
            }

            return userXpLog;
        }

        // PUT: api/UserXpLogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserXpLog(int id, UserXpLog userXpLog)
        {
            if (id != userXpLog.LogId)
            {
                return BadRequest();
            }

            _context.Entry(userXpLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserXpLogExists(id))
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

        // POST: api/UserXpLogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserXpLog>> PostUserXpLog(UserXpLog userXpLog)
        {
            _context.UserXpLog.Add(userXpLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserXpLog", new { id = userXpLog.LogId }, userXpLog);
        }

        // DELETE: api/UserXpLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserXpLog(int id)
        {
            var userXpLog = await _context.UserXpLog.FindAsync(id);
            if (userXpLog == null)
            {
                return NotFound();
            }

            _context.UserXpLog.Remove(userXpLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserXpLogExists(int id)
        {
            return _context.UserXpLog.Any(e => e.LogId == id);
        }
    }
}
