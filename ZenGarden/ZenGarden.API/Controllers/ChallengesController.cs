﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChallengesController(IChallengeService challengeService) : ControllerBase
{
    private readonly IChallengeService _challengeService =
        challengeService ?? throw new ArgumentNullException(nameof(challengeService));

    [HttpGet]
    public async Task<IActionResult> GetChallenge()
    {
        var challenge = await _challengeService.GetAllChallengeAsync();
        Console.WriteLine(challenge.ToList());
        return Ok(challenge);
    }


    [HttpGet("{challengeId:int}")]
    public async Task<IActionResult> GetChallenge(int challengeId)
    {
        var user = await _challengeService.GetChallengeByIdAsync(challengeId);
        return Ok(user);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeDto challengeDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId)) return Unauthorized();

        var challenge = await _challengeService.CreateChallengeAsync(userId, challengeDto);

        return CreatedAtAction(nameof(CreateChallenge), new { id = challenge.ChallengeId }, challenge);
    }
    
    [HttpPost("{challengeId:int}/join")]
    [Authorize]
    public async Task<IActionResult> JoinChallenge(int challengeId, [FromBody] JoinChallengeDto joinDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized();

        var result = await _challengeService.JoinChallengeAsync(userId, challengeId, joinDto.UserTreeId, joinDto.TaskTypeId);
    
        if (!result) return BadRequest("Failed to join the challenge.");

        return Ok(new { Message = "Joined challenge successfully!" });
    }


    [HttpPut]
    public async Task<IActionResult> PutUChallenge(ChallengeDto challenge)
    {
        await _challengeService.UpdateChallengeAsync(challenge);

        var i = await _challengeService.GetChallengeByIdAsync(challenge.ChallengeId);
        return Ok(i);
    }
}