using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/wallets")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("balance/{userId:int}")]
    public async Task<ActionResult<decimal>> GetBalance(int userId)
    {
        try
        {
            var balance = await _walletService.GetBalanceAsync(userId);
            return Ok(balance);
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<WalletDto>> GetWallet(int userId)
    {
        try
        {
            var wallet = await _walletService.GetWalletAsync(userId);
            return Ok(wallet);
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("{userId:int}/lock")]
    public async Task<IActionResult> LockWallet(int userId)
    {
        try
        {
            await _walletService.LockWalletAsync(userId);
            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("{userId:int}/unlock")]
    public async Task<IActionResult> UnlockWallet(int userId)
    {
        try
        {
            await _walletService.UnlockWalletAsync(userId);
            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<WalletDto>>> GetAllWallets()
    {
        try
        {
            var wallets = await _walletService.GetAllWalletAsync();
            return Ok(wallets);
        }
        catch
        {
            return StatusCode(500);
        }
    }
}