using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController(ITransactionsService transactionsService) : ControllerBase
{
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetAllTransactionsByUserId(int userId)
    {
        try
        {
            var transactions = await transactionsService.GetAllTransactionsByUserIdAsync(userId);
            return Ok(transactions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Transactions()
    {
        try
        {
            var transactions = await transactionsService.GetTransactionsAsync();
            return Ok(transactions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost("mark-old-pending")]
    public async Task<IActionResult> MarkOldPendingTransactionsAsFailed()
    {
        try
        {
            await transactionsService.MarkOldPendingTransactionsAsFailedAsync();
            return Ok("Old pending transactions marked as failed.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}