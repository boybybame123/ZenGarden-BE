namespace ZenGarden.Domain.DTOs;

public class CreateTaskDto
{
    public int? FocusMethodId { get; set; } // Nếu null, sẽ gọi AI đề xuất phương pháp mới
    public int TaskTypeId { get; set; }
    public int UserTreeId { get; set; }
    public string? TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? WorkDuration { get; set; } // Người dùng có thể nhập hoặc để mặc định
    public int? BreakTime { get; set; } // Người dùng có thể nhập hoặc để mặc định
}