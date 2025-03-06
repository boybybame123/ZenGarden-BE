using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class CreatePaymentRequest
    {
        public int UserId { get; set; }
        public int WalletId { get; set; }
        public int PackageId { get; set; }
    }
}
