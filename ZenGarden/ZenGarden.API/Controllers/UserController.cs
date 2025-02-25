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


}
