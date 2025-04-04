namespace Ecommerce.Application.SalesOrders.DTOs;

public class OrderProductDto
{
    public required string Name { get; set; } = null!;
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
    public required decimal Subtotal { get; set; }
}
