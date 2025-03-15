using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Domain.DTOs
{
    public class WalletDto
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public decimal? Balance { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Currency { get; set; } = "VND";
        public bool IsLocked { get; set; } = false;
        public DateTime? LastTransactionAt { get; set; }
        public virtual ICollection<TransactionsDto> Transactions { get; set; } = new List<TransactionsDto>();
        public virtual UserDto? User { get; set; }
    }
}
