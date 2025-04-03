using NpgsqlTypes;

namespace Ecommerce.Domain;

public class Product : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal RegularPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required int Quantity { get; set; }
    public required bool Active { get; set; }
    public required int Length { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required int Weight { get; set; }
    public NpgsqlTsVector SearchVector { get; set; } = null!;
    public required string ShopId { get; set; }

    public User Shop { get; set; } = null!;
    public ICollection<Subcategory> Subcategories { get; set; } = [];
    public ICollection<ProductPhoto> Photos { get; set; } = [];
}
