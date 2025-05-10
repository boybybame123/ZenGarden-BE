namespace ZenGarden.Domain.DTOs;

public class UserResponseDto
{
    public int UserId { get; set; }
    public int? RoleId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 