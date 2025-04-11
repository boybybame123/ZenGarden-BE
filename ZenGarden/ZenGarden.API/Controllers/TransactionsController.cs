using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

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

}