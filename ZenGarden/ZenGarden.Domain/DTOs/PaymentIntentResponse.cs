using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class PaymentIntentResponse
    {
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public decimal Amount { get; set; }           // Số tiền thực tế (VND)
        public decimal PackageAmount { get; set; }    // Số lượng Zen/Ví được cộng
        public string Currency { get; set; }
        public string PackageName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class CheckoutResponse
    {
        public string CheckoutUrl { get; set; } // URL để redirect user đến trang thanh toán
        public string SessionId { get; set; }   // Checkout Session ID
        public string PaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public string PackageName { get; set; }
    }

}
