using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class PurchaseRequest
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }
    }
}
