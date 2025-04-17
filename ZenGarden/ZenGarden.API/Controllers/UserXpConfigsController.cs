using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserXpConfigsController(IUserXpConfigService userXpConfigService) : ControllerBase
{
    private readonly IUserXpConfigService _userXpConfigService =
        userXpConfigService ?? throw new ArgumentNullException(nameof(userXpConfigService));

    [HttpGet]
    public async Task<IActionResult> GetUserXpConfigs()
    {
        var userXpConfigs = await _userXpConfigService.GetAllUserXpConfigsAsync();
        return Ok(userXpConfigs);
    }

    [HttpGet("{userXpConfigId:int}")]
    public async Task<IActionResult> GetUserXpConfigById(int userXpConfigId)
    {
        var userXpConfig = await _userXpConfigService.GetUserXpConfigByIdAsync(userXpConfigId);
        if (userXpConfig == null) return NotFound();
        return Ok(userXpConfig);
    }

    [HttpDelete("{userXpConfigId:int}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteUserXpConfig(int userXpConfigId)
    {
        await _userXpConfigService.DeleteUserXpConfigAsync(userXpConfigId);
        return Ok(new { message = "UserXpConfig deleted successfully" });
    }

    [HttpPut("update-UserXpConfig")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateUserXpConfig(UserXpConfigDto userXpConfig)
    {
        await _userXpConfigService.UpdateUserXpConfigAsync(userXpConfig);
        var i = await _userXpConfigService.GetUserXpConfigByIdAsync(userXpConfig.LevelId);
        return Ok(new { message = "UserXpConfig updated successfully", i });
    }

    [HttpPost]
    public async Task<IActionResult> PostUserConfig(UserXpConfigDto userConfig)
    {
        await _userXpConfigService.CreateUserXpConfigAsync(userConfig);
        var i = await _userXpConfigService.GetUserXpConfigByIdAsync(userConfig.LevelId);

        return Ok(new { message = "UserXpConfig updated successfully", i });
    }
}