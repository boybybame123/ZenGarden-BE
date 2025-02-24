using FluentValidation;
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
    private readonly IValidator<LoginModel> _validator;

    public AuthController(IUserService userService, ITokenService tokenService, IValidator<LoginModel> validator)
    {
        _userService = userService;
        _tokenService = tokenService;
        _validator = validator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        var validationResult = await _validator.ValidateAsync(loginModel);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var user = await _userService.ValidateUserAsync(loginModel.Email, loginModel.Phone, loginModel.Password);
        if (user == null)
        {
            return Unauthorized(new ErrorResponse("Invalid credentials."));
        }

        var token = _tokenService.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
    {
        var user = await _userService.GetUserByRefreshTokenAsync(model.RefreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        var newAccessToken = _tokenService.GenerateJwtToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _userService.UpdateUserRefreshTokenAsync(user.UserId, newRefreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken });
    }
}