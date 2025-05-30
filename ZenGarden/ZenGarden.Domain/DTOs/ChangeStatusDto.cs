namespace ZenGarden.Domain.DTOs;

public class ChangeStatusDto
{
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
} 