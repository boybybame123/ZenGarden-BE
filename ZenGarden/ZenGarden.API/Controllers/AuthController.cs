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
    IValidator<RegisterDto> registerValidator,
    IValidator<LoginDto> loginValidator,
    IValidator<ChangePasswordDto> changePasswordValidator)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        Console.WriteLine("=== LOGIN START ===");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        // Run validation and user validation in parallel
        var validationTask = loginValidator.ValidateAsync(loginDto);
        var userTask = userService.ValidateUserAsync(loginDto.Email, loginDto.Phone, loginDto.Password);
        
        await Task.WhenAll(validationTask, userTask);
        
        var validationResult = validationTask.Result;
        var user = userTask.Result;
        
        sw.Stop();
        Console.WriteLine($"[Login] Validation + User Validation: {sw.ElapsedMilliseconds}ms");
        
        if (!validationResult.IsValid) 
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            
        if (user == null)
            return Unauthorized(new { error = "Invalid credentials." });

        // Generate tokens and update refresh token in parallel
        sw.Restart();
        var tokenTask = Task.Run(() => tokenService.GenerateJwtToken(user));
        var updateTokenTask = userService.UpdateUserRefreshTokenAsync(
            user.UserId, 
            tokenTask.Result.RefreshToken, 
            DateTime.UtcNow.AddDays(7)
        );
        
        await Task.WhenAll(tokenTask, updateTokenTask);
        var tokens = tokenTask.Result;
        
        sw.Stop();
        Console.WriteLine($"[Login] Token Generation + Update: {sw.ElapsedMilliseconds}ms");

        // Fire and forget login tasks in true background
        _ = Task.Run(() => userService.OnUserLoginAsync(user.UserId));

        return Ok(new { tokens.Token, tokens.RefreshToken });
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

        var tokens = tokenService.GenerateJwtToken(user);

        await userService.UpdateUserRefreshTokenAsync(user.UserId, tokens.RefreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new { tokens.Token, tokens.RefreshToken });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var validationResult = await registerValidator.ValidateAsync(registerDto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var user = await userService.RegisterUserAsync(registerDto);
        if (user == null) throw new InvalidOperationException("Registration failed.");

        var tokens = tokenService.GenerateJwtToken(user);

        await userService.UpdateUserRefreshTokenAsync(user.UserId, tokens.RefreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new { tokens.Token, tokens.RefreshToken });
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