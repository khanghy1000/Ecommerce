using VNPAY.NET.Models;

namespace Ecommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentUrl(double money, string description);
    Task<PaymentResult?> GetPaymentResult();
}
