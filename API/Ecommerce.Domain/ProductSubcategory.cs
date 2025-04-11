namespace Ecommerce.Domain;

public class ProductSubcategory
{
    public int ProductId { get; set; }
    public int SubcategoryId { get; set; }

    public Product Product { get; set; } = null!;
    public Subcategory Subcategory { get; set; } = null!;
}
