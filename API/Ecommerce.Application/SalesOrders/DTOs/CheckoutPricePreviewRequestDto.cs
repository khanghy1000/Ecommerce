using Ecommerce.Domain;

namespace Ecommerce.Application.SalesOrders.DTOs;

public class CheckoutPricePreviewRequestDto
{
    public int? CouponId { get; set; }
    public string ShippingName { get; set; } = "";
    public string ShippingPhone { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public int ShippingWardId { get; set; }
}
