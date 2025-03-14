using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Domain.DTOs
{
    public class BuyItemDto
    {
        public int ItemId { get; set; }

        public int UserId { get; set; }

    }
}
