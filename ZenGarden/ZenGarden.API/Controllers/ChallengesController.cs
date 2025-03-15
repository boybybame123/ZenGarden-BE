using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;
using ZenGarden.Domain.DTOs;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChallengesController(IChallengeService challengeService) : ControllerBase
{
    private readonly IChallengeService _challengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));

    [HttpGet]
    public async Task<IActionResult> GetChallenge()
    {
        var challenge = await _challengeService.GetAllChallengeAsync();
        Console.WriteLine(challenge.ToList());
        return Ok(challenge);
    }


    [HttpGet("{challengeId}")]
    public async Task<IActionResult> GetChallenge(int challengeId)
    {
        var user = await _challengeService.GetChallengeByIdAsync(challengeId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> PostChallenge(ChallengeDto challenge)
    {
        await _challengeService.CreateChallengeAsync(challenge);
        var i = await _challengeService.GetChallengeByIdAsync(challenge.ChallengeId);
        return Ok(i);
    }
    [HttpPut]
    public async Task<IActionResult> PutUChallenge(ChallengeDto challenge)
    {
        await _challengeService.UpdateChallengeAsync(challenge);

        var i = await _challengeService.GetChallengeByIdAsync(challenge.ChallengeId);
        return Ok(i);
    }

}
