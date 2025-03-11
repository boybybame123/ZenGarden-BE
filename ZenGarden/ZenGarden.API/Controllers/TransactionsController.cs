using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController : ControllerBase
{
    private readonly ZenGardenContext _context;

    public TransactionsController(ZenGardenContext context)
    {
        _context = context;
    }

    // GET: api/Transactions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transactions>>> GetTransactions()
    {
        return await _context.Transactions.ToListAsync();
    }

    // GET: api/Transactions/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Transactions>> GetTransactions(int id)
    {
        var transactions = await _context.Transactions.FindAsync(id);

        if (transactions == null) return NotFound();

        return transactions;
    }

    // PUT: api/Transactions/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTransactions(int id, Transactions transactions)
    {
        if (id != transactions.TransactionId) return BadRequest();

        _context.Entry(transactions).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TransactionsExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/Transactions
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Transactions>> PostTransactions(Transactions transactions)
    {
        _context.Transactions.Add(transactions);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTransactions", new { id = transactions.TransactionId }, transactions);
    }

    // DELETE: api/Transactions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransactions(int id)
    {
        var transactions = await _context.Transactions.FindAsync(id);
        if (transactions == null) return NotFound();

        _context.Transactions.Remove(transactions);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TransactionsExists(int id)
    {
        return _context.Transactions.Any(e => e.TransactionId == id);
    }
}