namespace ZenGarden.Domain.DTOs;

public class TradeDto
{
    public int requesterId { get; set; }
    public int requesterTreeId { get; set; }
    public int requestDesiredTreeId { get; set; }
}