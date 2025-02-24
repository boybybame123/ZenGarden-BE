using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.API.Response;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserService userService,
    ITokenService tokenService,
    IValidator<LoginDto> loginValidator,
    IValidator<RegisterDto> registerValidator)
    : Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var validationResult = await loginValidator.ValidateAsync(loginDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var user = await userService.ValidateUserAsync(loginDto.Email, loginDto.Phone, loginDto.Password);
        if (user == null)
        {
            return Unauthorized(new { error = "Invalid credentials." });
        }

        var accessToken = tokenService.GenerateJwtToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        await userService.UpdateUserRefreshTokenAsync(user.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new { Token = accessToken, RefreshToken = refreshToken });
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token is required." });
        }

        var user = await userService.GetUserByRefreshTokenAsync(refreshTokenDto.RefreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Unauthorized(new { error = "Invalid or expired refresh token." });
        }

        await userService.RemoveRefreshTokenAsync(user.UserId);

        var newAccessToken = tokenService.GenerateJwtToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        await userService.UpdateUserRefreshTokenAsync(user.UserId, newRefreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken });
    }

    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var validationResult = await registerValidator.ValidateAsync(registerDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var user = await userService.RegisterUserAsync(registerDto);
        if (user == null)
        {
            return BadRequest(new ErrorResponse("Registration failed."));
        }

        var accessToken = tokenService.GenerateJwtToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        await userService.UpdateUserRefreshTokenAsync(user.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new { Token = accessToken, RefreshToken = refreshToken });
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { error = "Invalid or missing user ID." });
        }

        var user = await userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { error = "User not found." });
        }

        await userService.RemoveRefreshTokenAsync(userId);
        return Ok(new { message = "Logged out successfully." });
    }
    
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var user = await userService.GetUserByIdAsync(parsedUserId);
        if (user == null)
        {
            return NotFound(new ErrorResponse("User not found."));
        }
        
        var userResponse = new UserResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status,
            Role = user.Role.RoleName
        };

        return Ok(userResponse);
    }
}