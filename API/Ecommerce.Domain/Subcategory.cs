namespace Ecommerce.Domain;

public class Subcategory : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = [];
}
