namespace Ecommerce.Domain;

public class ProductDiscount : BaseEntity
{
    public int Id { get; set; }
    public required decimal DiscountPrice { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required int ProductId { get; set; }

    public Product Product { get; set; } = null!;
}
