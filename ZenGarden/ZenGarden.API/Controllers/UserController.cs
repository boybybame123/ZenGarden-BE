using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    // GET: api/User
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }
    [HttpGet("filter")]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter)
    {
        var users = await userService.GetAllUserFilterAsync(filter);
        return Ok(users);
    }
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
    [HttpDelete("{userId}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
            await userService.DeleteUserAsync(userId);
            return Ok(new { message = "User deleted successfully" });
    }
    [HttpGet("change-active/{userId}")]
    [Produces("application/json")]
    public async Task<IActionResult> ChangeUserisActive(int userId)
    {
        await userService.ChangeUserisActiveAsync(userId);
        return Ok(new { message = "User active status changed successfully" });
    }
    [HttpPost("update user")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateUser(UserDto user)
    {
        await userService.UpdateUserAsync(user);
        return Ok(new { message = "User updated successfully" });
    }
}
