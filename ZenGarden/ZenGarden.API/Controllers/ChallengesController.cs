using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChallengesController(IChallengeService challangeService) : ControllerBase
{
    private readonly IChallengeService _challangeService = challangeService ?? throw new ArgumentNullException(nameof(challangeService));

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _challangeService.GetAllChallengeAsync();
        return Ok(users);
    }
}