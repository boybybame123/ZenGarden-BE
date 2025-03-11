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
    public class TradeHistoriesController : ControllerBase
    {
        private readonly ZenGardenContext _context;

        public TradeHistoriesController(ZenGardenContext context)
        {
            _context = context;
        }

        // GET: api/TradeHistories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TradeHistory>>> GetTradeHistory()
        {
            return await _context.TradeHistory.ToListAsync();
        }

        // GET: api/TradeHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TradeHistory>> GetTradeHistory(int id)
        {
            var tradeHistory = await _context.TradeHistory.FindAsync(id);

            if (tradeHistory == null)
            {
                return NotFound();
            }

            return tradeHistory;
        }

        // PUT: api/TradeHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTradeHistory(int id, TradeHistory tradeHistory)
        {
            if (id != tradeHistory.TradeId)
            {
                return BadRequest();
            }

            _context.Entry(tradeHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TradeHistoryExists(id))
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

        // POST: api/TradeHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TradeHistory>> PostTradeHistory(TradeHistory tradeHistory)
        {
            _context.TradeHistory.Add(tradeHistory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTradeHistory", new { id = tradeHistory.TradeId }, tradeHistory);
        }

        // DELETE: api/TradeHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTradeHistory(int id)
        {
            var tradeHistory = await _context.TradeHistory.FindAsync(id);
            if (tradeHistory == null)
            {
                return NotFound();
            }

            _context.TradeHistory.Remove(tradeHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TradeHistoryExists(int id)
        {
            return _context.TradeHistory.Any(e => e.TradeId == id);
        }
    }
}
