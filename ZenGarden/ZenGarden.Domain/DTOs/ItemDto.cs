using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Domain.DTOs
{
    public class ItemDto
    {
        public int ItemId { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Rarity { get; set; }

        public decimal? Cost { get; set; }

        public bool? Limited { get; set; }

        public DateTime? CreatedAt { get; set; }



        public virtual ItemDetail ItemDetail { get; set; }
    }
}
