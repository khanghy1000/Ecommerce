namespace Ecommerce.Application.SalesOrders.DTOs;

public class CheckoutPricePreviewResponseDto
{
    public required decimal Subtotal { get; set; }
    public required decimal ShippingFee { get; set; }
    public decimal ProductDiscountAmount { get; set; }
    public decimal ShippingDiscountAmount { get; set; }
    public required decimal Total { get; set; }
    public string? AppliedProductCoupon { get; set; }
    public string? AppliedShippingCoupon { get; set; }
}
