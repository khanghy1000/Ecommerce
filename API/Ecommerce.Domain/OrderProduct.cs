namespace Ecommerce.Domain;

public class OrderProduct : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; } = null!;
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
    public required decimal Subtotal { get; set; }
    public required int OrderId { get; set; }
    
    public SalesOrder Order { get; set; } = null!;
}