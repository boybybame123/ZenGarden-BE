using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;
public class UserTreeDto
{
    public int? UserId { get; set; }
    public required string Name { get; set; }
    public TreeStatus TreeStatus { get; set; } = TreeStatus.Growing;
}
