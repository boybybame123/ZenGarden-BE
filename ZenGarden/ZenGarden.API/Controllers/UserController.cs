using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }



    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpDelete("{userId:int}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        await _userService.DeleteUserAsync(userId);
        return Ok(new { message = "User deleted successfully" });
    }


    [HttpPut("update-user")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateUser(UpdateUserDTO user)
    {
        await _userService.UpdateUserAsync(user);
        return Ok(new { message = "User updated successfully" });
    }
}