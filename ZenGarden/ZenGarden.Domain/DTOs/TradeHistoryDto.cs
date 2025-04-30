using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs
{
    public class TradeHistoryDto
    {
        public int TradeId { get; set; }
        public int? TreeAid { get; set; }
        public int? DesiredTreeAID { get; set; }
        public int? TreeOwnerAid { get; set; }
        public int? TreeOwnerBid { get; set; }
        public decimal? TradeFee { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public TradeStatus Status { get; set; }
        public int? FinalTreeId { get; set; }
    }
}
