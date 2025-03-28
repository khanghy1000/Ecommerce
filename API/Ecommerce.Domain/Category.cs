namespace Ecommerce.Domain;

public class Category : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? ParentId { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> InverseParent { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
