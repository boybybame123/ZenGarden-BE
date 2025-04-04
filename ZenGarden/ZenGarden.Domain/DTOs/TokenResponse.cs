namespace ZenGarden.Domain.DTOs;

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}