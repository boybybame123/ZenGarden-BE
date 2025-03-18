using Microsoft.AspNetCore.Http;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class ItemDto
{
  

    public string? Name { get; set; }

    public string?Type { get; set; }

    public string? Rarity { get; set; }

    public decimal? Cost { get; set; }

    public IFormFile? File { get; set; }
    public ItemStatus Status { get; set; }



    public virtual ItemDetailDto? ItemDetail { get; set; }
}