namespace ZenGarden.Domain.DTOs;

public class LoginDto
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public required string Password { get; set; }
}