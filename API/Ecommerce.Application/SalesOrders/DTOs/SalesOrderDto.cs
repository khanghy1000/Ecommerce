using Ecommerce.Domain;

namespace Ecommerce.Application.SalesOrders.DTOs;

public class SalesOrderDto
{
    public int Id { get; set; }
    public DateTime OrderTime { get; set; }
    public decimal Total { get; set; }
    public string UserId { get; set; }
    public int? CouponId { get; set; }
    public string ShippingOrderCode { get; set; }
    public string ShippingName { get; set; }
    public string ShippingPhone { get; set; }
    public string ShippingAddress { get; set; }
    public int ShippingWardId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public SalesOrderStatus Status { get; set; }
    public ICollection<OrderProductDto> OrderProducts { get; set; } = [];
}
