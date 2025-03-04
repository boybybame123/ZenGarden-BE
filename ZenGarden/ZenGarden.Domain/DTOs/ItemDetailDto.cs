using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class ItemDetailDto
    {
        public int ItemDetailId { get; set; }

        public int? ItemId { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public string Stats { get; set; }

        public string Requirements { get; set; }

        public string SpecialEffects { get; set; }

        public string DurationType { get; set; }

        public int? Duration { get; set; }

        public int? Cooldown { get; set; }

        public int? MaxStack { get; set; }

        public string Tags { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
