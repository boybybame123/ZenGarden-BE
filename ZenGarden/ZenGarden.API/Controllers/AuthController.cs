using Microsoft.AspNetCore.Mvc;
using ZenGarden.API.Models;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel loginModel)
    {
        
        try
        {
            var user = _userService.ValidateUser(loginModel.Email, loginModel.Phone, loginModel.Password);
            if (user == null)
            {
                return Unauthorized(new ErrorResponse("Invalid credentials."));
            }

            var token = _tokenService.GenerateJwtToken(user);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse("An error occurred while processing your request.", ex.Message));
        }
    }
}