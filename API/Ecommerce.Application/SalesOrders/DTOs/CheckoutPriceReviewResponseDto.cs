namespace Ecommerce.Application.SalesOrders.DTOs;

public class CheckoutPriceReviewResponseDto
{
    public required decimal Subtotal { get; set; }
    public required decimal ShippingFee { get; set; }
    public required decimal Total { get; set; }
}
