namespace ZenGarden.Domain.DTOs;

public class CreateUserTreeDto
{
    public required int UserId { get; set; }
    public required string Name { get; set; }
}