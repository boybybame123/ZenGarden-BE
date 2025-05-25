using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.API.Middleware;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs.FocusTracking;

namespace ZenGarden.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FocusTrackingController(IFocusTrackingService focusTrackingService) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<FocusTrackingDto>> StartTracking([FromBody] CreateFocusTrackingDto dto)
    {
        var userId = HttpContext.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found");
        var tracking = await focusTrackingService.StartTrackingAsync(userId, dto);
        return Ok(tracking);
    }

    [HttpPut("{trackingId:int}/end")]
    public async Task<ActionResult<FocusTrackingDto>> EndTracking(int trackingId, [FromBody] UpdateFocusTrackingDto dto)
    {
        var tracking = await focusTrackingService.EndTrackingAsync(trackingId, dto);
        return Ok(tracking);
    }

    [HttpGet("{trackingId:int}")]
    public async Task<ActionResult<FocusTrackingDto>> GetTracking(int trackingId)
    {
        var tracking = await focusTrackingService.GetTrackingByIdAsync(trackingId);
        return Ok(tracking);
    }

    [HttpGet("user")]
    public async Task<ActionResult<List<FocusTrackingDto>>> GetUserTrackings()
    {
        var userId = HttpContext.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found");
        var trackings = await focusTrackingService.GetUserTrackingsAsync(userId);
        return Ok(trackings);
    }

    [HttpPost("activity")]
    public async Task<ActionResult<FocusActivityDto>> AddActivity([FromBody] CreateFocusActivityDto dto)
    {
        var activity = await focusTrackingService.AddActivityAsync(dto);
        return Ok(activity);
    }

    [HttpGet("{trackingId:int}/activities")]
    public async Task<ActionResult<List<FocusActivityDto>>> GetTrackingActivities(int trackingId)
    {
        var activities = await focusTrackingService.GetTrackingActivitiesAsync(trackingId);
        return Ok(activities);
    }

    [HttpGet("{trackingId:int}/xp")]
    public async Task<ActionResult<double>> CalculateXpEarned(int trackingId)
    {
        var xp = await focusTrackingService.CalculateXpEarnedAsync(trackingId);
        return Ok(xp);
    }
}