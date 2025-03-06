using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ZenGarden.Domain.Config;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Shared.Helpers;

public static class JwtHelper
{
    public static string GenerateToken(Users user, JwtSettings jwtSettings)
    {
        if (string.IsNullOrEmpty(jwtSettings.Key) || jwtSettings.Key.Length < 32)
            throw new InvalidOperationException("JWT Key must be at least 32 characters long.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiresInMinutes = jwtSettings.ExpiresInMinutes > 0 ? jwtSettings.ExpiresInMinutes : 60;
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new("sub", user.UserId.ToString()),
            new("jti", Guid.NewGuid().ToString()),
            new("email", user.Email),
            new("userId", user.UserId.ToString())
        };

        if (!string.IsNullOrEmpty(user.UserName))
            claims.Add(new Claim("name", user.UserName));

        if (!string.IsNullOrEmpty(user.Role?.RoleName))
            claims.Add(new Claim("role", user.Role.RoleName));

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: now.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}