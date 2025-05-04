namespace Ecommerce.Domain;

public class PopularProduct
{
    public required int CategoryId { get; set; }
    public required int ProductId { get; set; }
    public required int SalesCount { get; set; }

    public Product Product { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
