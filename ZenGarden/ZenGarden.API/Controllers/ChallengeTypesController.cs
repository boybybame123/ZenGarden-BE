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
    public class ChallengeTypesController : ControllerBase
    {
        private readonly ZenGardenContext _context;

        public ChallengeTypesController(ZenGardenContext context)
        {
            _context = context;
        }

        // GET: api/ChallengeTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChallengeType>>> GetChallengeType()
        {
            return await _context.ChallengeType.ToListAsync();
        }

        // GET: api/ChallengeTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChallengeType>> GetChallengeType(int id)
        {
            var challengeType = await _context.ChallengeType.FindAsync(id);

            if (challengeType == null)
            {
                return NotFound();
            }

            return challengeType;
        }

        // PUT: api/ChallengeTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChallengeType(int id, ChallengeType challengeType)
        {
            if (id != challengeType.ChallengeTypeId)
            {
                return BadRequest();
            }

            _context.Entry(challengeType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChallengeTypeExists(id))
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

        // POST: api/ChallengeTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ChallengeType>> PostChallengeType(ChallengeType challengeType)
        {
            _context.ChallengeType.Add(challengeType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetChallengeType", new { id = challengeType.ChallengeTypeId }, challengeType);
        }

        // DELETE: api/ChallengeTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallengeType(int id)
        {
            var challengeType = await _context.ChallengeType.FindAsync(id);
            if (challengeType == null)
            {
                return NotFound();
            }

            _context.ChallengeType.Remove(challengeType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChallengeTypeExists(int id)
        {
            return _context.ChallengeType.Any(e => e.ChallengeTypeId == id);
        }
    }
}
