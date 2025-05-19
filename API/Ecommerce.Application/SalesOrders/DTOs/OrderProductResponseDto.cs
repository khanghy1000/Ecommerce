using Ecommerce.Domain;

namespace Ecommerce.Application.SalesOrders.DTOs;

public class OrderProductResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public ICollection<ProductPhoto> Photos { get; set; } = [];
}
