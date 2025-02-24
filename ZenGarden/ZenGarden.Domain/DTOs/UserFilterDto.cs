namespace ZenGarden.Domain.DTOs;

public class UserFilterDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Status { get; set; }
    public string? Role { get; set; }
    public int PageNumber { get; set; } = 1;
    public string? Search { get; set; }
    public bool UserByDescending { get; set; } = false;
}