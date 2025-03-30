using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Domain;

public class Product : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal RegularPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required int Quantity { get; set; }
    public required ProductStatus ProductStatus { get; set; }
    public required string ShopId { get; set; }

    public User Shop { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = [];
}
