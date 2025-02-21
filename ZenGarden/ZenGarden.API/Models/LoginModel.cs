namespace ZenGarden.API.Models;

public class LoginModel
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public required string Password { get; set; }
}