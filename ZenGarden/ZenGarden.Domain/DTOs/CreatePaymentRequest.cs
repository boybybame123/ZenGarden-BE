namespace ZenGarden.Domain.DTOs;

public class CreatePaymentRequest
{
    public int UserId { get; set; }
    public int WalletId { get; set; }
    public int PackageId { get; set; }
}