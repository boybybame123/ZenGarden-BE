using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITokenService
{
    string GenerateJwtToken(Users user);
    string GenerateRefreshToken();
}