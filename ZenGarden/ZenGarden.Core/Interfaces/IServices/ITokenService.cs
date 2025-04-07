using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITokenService
{
    TokenResponse GenerateJwtToken(Users user);
    string GenerateRefreshToken();
}