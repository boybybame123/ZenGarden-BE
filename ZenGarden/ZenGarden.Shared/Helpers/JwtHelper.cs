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
        if (jwtSettings.Key.Length < 32)
            throw new InvalidOperationException("JWT Key must be at least 32 characters long.");

        var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);
        var key = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresInMinutes = jwtSettings.ExpiresInMinutes > 0 ? jwtSettings.ExpiresInMinutes : 60;
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };


        if (!string.IsNullOrEmpty(user.UserName)) claims.Add(new Claim(ClaimTypes.Name, user.UserName));

        if (user.Role?.RoleName != null) claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: now.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}