using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs
{
    public class UpdateItemDto
    {

        public int ItemId { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }

        public string Rarity { get; set; }

        public decimal? Cost { get; set; }

    
        public ItemStatus Status { get; set; }


    }
}
