using Ecommerce.Domain;

namespace Ecommerce.Application.Products.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal RegularPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    public ProductStatus ProductStatus { get; set; }
    public string ShopId { get; set; } = "";
    public string ShopName { get; set; } = "";
    public string ShopImageUrl { get; set; } = "";

    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
}
