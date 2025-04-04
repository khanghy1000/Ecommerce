using Ecommerce.Domain;

namespace Ecommerce.Application.SalesOrders.DTOs;

public class CheckoutResponseDto
{
    public string? PaymentUrl { get; set; }
    public List<SalesOrderDto> SalesOrders { get; set; } = [];
}
