using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Config;
using ZenGarden.Domain.Entities;
using ZenGarden.Shared.Helpers;

namespace ZenGarden.Core.Services;

public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value
                                                ?? throw new InvalidOperationException(
                                                    "JWT settings are missing in configuration.");

    public string GenerateJwtToken(Users user)
    {
        return JwtHelper.GenerateToken(user, _jwtSettings);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}