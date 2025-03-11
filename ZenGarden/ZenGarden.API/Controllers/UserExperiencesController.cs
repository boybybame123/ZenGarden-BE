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
    public class UserExperiencesController : ControllerBase
    {
        private readonly ZenGardenContext _context;

        public UserExperiencesController(ZenGardenContext context)
        {
            _context = context;
        }

        // GET: api/UserExperiences
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserExperience>>> GetUserExperience()
        {
            return await _context.UserExperience.ToListAsync();
        }

        // GET: api/UserExperiences/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserExperience>> GetUserExperience(int id)
        {
            var userExperience = await _context.UserExperience.FindAsync(id);

            if (userExperience == null)
            {
                return NotFound();
            }

            return userExperience;
        }

        // PUT: api/UserExperiences/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserExperience(int id, UserExperience userExperience)
        {
            if (id != userExperience.UserExperienceId)
            {
                return BadRequest();
            }

            _context.Entry(userExperience).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExperienceExists(id))
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

        // POST: api/UserExperiences
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserExperience>> PostUserExperience(UserExperience userExperience)
        {
            _context.UserExperience.Add(userExperience);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserExperience", new { id = userExperience.UserExperienceId }, userExperience);
        }

        // DELETE: api/UserExperiences/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserExperience(int id)
        {
            var userExperience = await _context.UserExperience.FindAsync(id);
            if (userExperience == null)
            {
                return NotFound();
            }

            _context.UserExperience.Remove(userExperience);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExperienceExists(int id)
        {
            return _context.UserExperience.Any(e => e.UserExperienceId == id);
        }
    }
}
