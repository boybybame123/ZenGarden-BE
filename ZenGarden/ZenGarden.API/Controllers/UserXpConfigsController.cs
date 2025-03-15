using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserXpConfigsController(IUserXpConfigService userXpConfigService) : ControllerBase
{
    private readonly IUserXpConfigService _UserXpConfigService =
        userXpConfigService ?? throw new ArgumentNullException(nameof(userXpConfigService));

    [HttpGet]
    public async Task<IActionResult> GetUserXpConfigs()
    {
        var UserXpConfigs = await _UserXpConfigService.GetAllUserXpConfigsAsync();
        return Ok(UserXpConfigs);
    }

    [HttpGet("{UserXpConfigId}")]
    public async Task<IActionResult> GetUserXpConfigById(int UserXpConfigId)
    {
        var UserXpConfig = await _UserXpConfigService.GetUserXpConfigByIdAsync(UserXpConfigId);
        if (UserXpConfig == null) return NotFound();
        return Ok(UserXpConfig);
    }

    [HttpDelete("{UserXpConfigId}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteUserXpConfig(int UserXpConfigId)
    {
        await _UserXpConfigService.DeleteUserXpConfigAsync(UserXpConfigId);
        return Ok(new { message = "UserXpConfig deleted successfully" });
    }

    [HttpPut("update-UserXpConfig")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateUserXpConfig(UserXpConfigDto UserXpConfig)
    {
        await _UserXpConfigService.UpdateUserXpConfigAsync(UserXpConfig);
        var i = await _UserXpConfigService.GetUserXpConfigByIdAsync(UserXpConfig.LevelId);
        return Ok(new { message = "UserXpConfig updated successfully", i });
    }

    [HttpPost]
    public async Task<IActionResult> PostUserConfig(UserXpConfigDto userConfig)
    {
        await _UserXpConfigService.CreateUserXpConfigAsync(userConfig);
        var i = await _UserXpConfigService.GetUserXpConfigByIdAsync(userConfig.LevelId);

        return Ok(new { message = "UserXpConfig updated successfully", i });
    }
}