using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Domain.DTOs
{
    public class BagItemDto
    {
        public int BagItemId { get; set; }

        public int? BagId { get; set; }

        public int? ItemId { get; set; }

        public int? Quantity { get; set; }

        public bool isEquipped { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ItemDto? Item { get; set; }
    }
}
