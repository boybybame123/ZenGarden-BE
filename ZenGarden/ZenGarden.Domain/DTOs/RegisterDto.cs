namespace ZenGarden.Domain.DTOs;

public class RegisterDto
{
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public int? RoleId { get; set; }
}