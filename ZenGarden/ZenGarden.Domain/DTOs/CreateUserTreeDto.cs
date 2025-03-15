namespace ZenGarden.Domain.DTOs;
public class CreateUserTreeDto
{
    public int? UserId { get; set; }
    public required string Name { get; set; }
}
