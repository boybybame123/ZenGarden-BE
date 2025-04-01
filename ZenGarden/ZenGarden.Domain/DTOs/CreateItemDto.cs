using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs
{
    public class CreateItemDto
    {
      

        public string? Name { get; set; }


        public ItemType Type { get; set; }

    
        public string? Rarity { get; set; }

  
        public decimal? Cost { get; set; }

   
        public IFormFile? File { get; set; }

      
        public ItemStatus Status { get; set; }

        
        public virtual CreateItemDetailDto? ItemDetail { get; set; }



    }
}
