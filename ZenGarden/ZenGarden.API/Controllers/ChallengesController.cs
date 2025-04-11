using System.Security.Claims;
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

    [HttpGet("get-all")]
    public async Task<IActionResult> GetChallenge()
    {
        var challenge = await _challengeService.GetAllChallengesAsync();
        return Ok(challenge);
    }


    [HttpGet("get-by-id/{challengeId:int}")]
    public async Task<IActionResult> GetChallenge(int challengeId)
    {
        var user = await _challengeService.GetChallengeByIdAsync(challengeId);
        return Ok(user);
    }

    [HttpPost("create-challenge")]
    [Authorize]
    public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeDto challengeDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId)) return Unauthorized();

        var challenge = await _challengeService.CreateChallengeAsync(userId, challengeDto);

        return CreatedAtAction(nameof(GetChallenge), new { challengeId = challenge.ChallengeId }, challenge);
    }

    [HttpPost("tasks/{challengeId:int}")]
    public async Task<IActionResult> CreateTaskForChallenge(int challengeId, [FromBody] CreateTaskDto taskDto)
    {
        var createdTask = await challengeService.CreateTaskForChallengeAsync(challengeId, taskDto);
        return CreatedAtAction(nameof(CreateTaskForChallenge), new { taskId = createdTask.TaskId }, createdTask);
    }


    [HttpPost("join/{challengeId:int}")]
    [Authorize]
    public async Task<IActionResult> JoinChallenge(int challengeId, [FromBody] JoinChallengeDto joinDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized();

        var result = await _challengeService.JoinChallengeAsync(userId, challengeId, joinDto);

        if (!result) return BadRequest(new { Error = "You have already joined this challenge or it is closed." });

        return Ok(new { Message = "Joined challenge successfully!" });
    }

    [HttpPut("update-challenge")]
    public async Task<IActionResult> UpdateChallenge([FromBody] UpdateChallengeDto challenge)
    {
        await _challengeService.UpdateChallengeAsync(challenge);

        var i = await _challengeService.GetChallengeByIdAsync(challenge.ChallengeId);
        return Ok(i);
    }

    [HttpPut("leave/{challengeId:int}")]
    [Authorize]
    public async Task<IActionResult> LeaveChallenge(int challengeId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized();

        var success = await _challengeService.LeaveChallengeAsync(userId, challengeId);
        if (!success) return BadRequest("Failed to leave the challenge.");

        return Ok(new { Message = "You have successfully left the challenge." });
    }

    [HttpPut("cancel/{challengeId:int}")]
    [Authorize]
    public async Task<IActionResult> CancelChallenge(int challengeId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized();

        var success = await _challengeService.CancelChallengeAsync(challengeId, userId);
        if (!success) return BadRequest("Failed to cancel the challenge.");

        return Ok(new { Message = "Challenge has been canceled successfully." });
    }

    [HttpGet("{challengeId:int}/ranking")]
    [Authorize]
    public async Task<IActionResult> GetChallengeRankings(int challengeId)
    {
        var rankings = await _challengeService.GetChallengeRankingsAsync(challengeId);

        if (rankings.Count == 0)
            return NotFound("No participants found for this challenge.");

        return Ok(rankings);
    }

    [HttpGet("progress/{challengeId:int}")]
    [Authorize]
    public async Task<IActionResult> GetUserChallengeProgress(int challengeId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized();

        var progressDto = await challengeService.GetUserChallengeProgressAsync(userId, challengeId);
        if (progressDto == null)
            return NotFound(new { Message = "You are not part of this challenge." });

        return Ok(progressDto);
    }
    [HttpPut("change-status/{challengeId:int}")]
    [Authorize]
    public async Task<IActionResult> ChangeStatusChallenge(int challengeId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized();
        var result = await _challengeService.ChangeStatusChallenge(userId, challengeId);
        return Ok(result);
    }
}