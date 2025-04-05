namespace Ecommerce.Application.SalesOrders.DTOs;

public class CheckoutPricePreviewResponseDto
{
    public required decimal Subtotal { get; set; }
    public required decimal ShippingFee { get; set; }
    public required decimal Total { get; set; }
}
