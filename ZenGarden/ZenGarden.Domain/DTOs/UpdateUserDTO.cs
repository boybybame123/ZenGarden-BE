using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class UpdateUserDTO
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}