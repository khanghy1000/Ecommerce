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
    public string BuyerId { get; set; }
    public string BuyerName { get; set; }
    public string ShopId { get; set; }
    public string ShopName { get; set; }
    public string ShippingOrderCode { get; set; }
    public string ShippingName { get; set; }
    public string ShippingPhone { get; set; }
    public string ShippingAddress { get; set; }
    public int ShippingWardId { get; set; }
    public int ShippingDistrictId { get; set; }
    public int ShippingProvinceId { get; set; }
    public string ShippingWardName { get; set; }
    public string ShippingDistrictName { get; set; }
    public string ShippingProvinceName { get; set; }
    public string ProductCouponCode { get; set; }
    public string ShippingCouponCode { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public SalesOrderStatus Status { get; set; }
    public ICollection<OrderProductResponseDto> OrderProducts { get; set; } = [];
}
