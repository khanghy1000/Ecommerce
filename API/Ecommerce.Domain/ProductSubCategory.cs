namespace Ecommerce.Domain;

public class ProductSubCategory
{
    public int ProductId { get; set; }
    public int SubCategoryId { get; set; }

    public Product Product { get; set; } = null!;
    public Subcategory Subcategory { get; set; } = null!;
}
