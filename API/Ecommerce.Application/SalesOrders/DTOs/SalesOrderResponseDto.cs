using Ecommerce.Domain;

namespace Ecommerce.Application.SalesOrders.DTOs;

public class SalesOrderResponseDto
{
    public int Id { get; set; }
    public DateTime OrderTime { get; set; }
    public decimal Subtotal { get; set; }
    public int ShippingFee { get; set; }
    public decimal ProductDiscountAmount { get; set; }
    public decimal ShippingDiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string UserId { get; set; }
    public string ShippingOrderCode { get; set; }
    public string ShippingName { get; set; }
    public string ShippingPhone { get; set; }
    public string ShippingAddress { get; set; }
    public int ShippingWardId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public SalesOrderStatus Status { get; set; }
    public ICollection<OrderProductResponseDto> OrderProducts { get; set; } = [];
}
