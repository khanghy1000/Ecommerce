using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Domain;

public class Payment
{
    [Key]
    public long PaymentId { get; set; } // VNPay's PaymentId
    public bool IsSuccess { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public long VnpayTransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseDescription { get; set; }
    public string? TransactionCode { get; set; }
    public string? TransactionDescription { get; set; }
    public string? BankCode { get; set; }
    public string? BankTransactionId { get; set; }

    public ICollection<SalesOrder> SalesOrder { get; set; } = [];
}
