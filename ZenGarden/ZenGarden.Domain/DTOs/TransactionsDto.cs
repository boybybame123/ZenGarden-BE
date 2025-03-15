using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class TransactionsDto
{
    public int TransactionId { get; set; }
    public int? UserId { get; set; }
    public int? WalletId { get; set; }
    public int? PackageId { get; set; }
    public decimal? Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public string PaymentMethod { get; set; } = "";
    public string TransactionRef { get; set; } = "";
    public DateTime TransactionTime { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual UserDto User { get; set; }
    public virtual WalletDto Wallet { get; set; }
    public virtual PackageDto Package { get; set; }
}