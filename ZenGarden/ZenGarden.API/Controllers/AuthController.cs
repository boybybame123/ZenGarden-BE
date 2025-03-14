using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserService userService,
    ITokenService tokenService,
    IEmailService emailService,
    IValidator<LoginDto> loginValidator,
    IValidator<RegisterDto> registerValidator,
    IValidator<ChangePasswordDto> changePasswordValidator)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var validationResult = await loginValidator.ValidateAsync(loginDto);
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        var user = await userService.ValidateUserAsync(loginDto.Email, loginDto.Phone, loginDto.Password);
        if (user == null) return Unauthorized(new { error = "Invalid credentials." });

        var accessToken = tokenService.GenerateJwtToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        await userService.UpdateUserRefreshTokenAsync(user.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new { Token = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
            return BadRequest(new { error = "Refresh token is required." });

        var user = await userService.GetUserByRefreshTokenAsync(refreshTokenDto.RefreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized(new { error = "Invalid or expired refresh token." });

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
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var user = await userService.RegisterUserAsync(registerDto);
        if (user == null) return BadRequest(new ErrorResponse("Registration failed."));

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
            return Unauthorized(new { error = "Invalid or missing user ID." });

        var user = await userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound(new { error = "User not found." });

        await userService.RemoveRefreshTokenAsync(userId);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await userService.GetUserByEmailAsync(model.Email);
        if (user == null) return NotFound(new { error = "User with this email does not exist." });

        var otp = await userService.GenerateAndSaveOtpAsync(user.Email);
        await emailService.SendOtpEmailAsync(user.Email, otp);

        return Ok(new { message = "OTP has been sent to your email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var isResetSuccessful = await userService.ResetPasswordAsync(model.Email, model.Otp, model.NewPassword);
        if (!isResetSuccessful) return BadRequest(new { error = "Invalid or expired OTP." });

        return Ok(new { message = "Password reset successfully." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var validationResult = await changePasswordValidator.ValidateAsync(model);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId)) return Unauthorized();

        var isChanged = await userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
        if (!isChanged) return BadRequest(new { error = "Old password is incorrect." });

        return Ok(new { message = "Password changed successfully." });
    }
}