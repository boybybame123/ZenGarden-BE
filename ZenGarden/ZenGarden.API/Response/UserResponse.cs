using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.API.Response;

public class UserResponse(Users user)
{
    public int UserId { get; set; } = user.UserId;
    public string FullName { get; set; } = user.UserName;
    public string Email { get; set; } = user.Email;
    public string Phone { get; set; } = user.Phone;
    public UserStatus Status { get; set; } = user.Status;
    public string Role { get; set; } = user.Role?.RoleName ?? "Unknown";
    public DateTime CreatedAt { get; set; } = user.CreatedAt;
    public DateTime UpdatedAt { get; set; } = user.UpdatedAt;
}