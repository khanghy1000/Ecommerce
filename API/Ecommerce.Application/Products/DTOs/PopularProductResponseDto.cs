using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Domain;

namespace Ecommerce.Application.Products.DTOs;

public class PopularProductResponseDto
{
    public int CategoryId { get; set; }
    public int ProductId { get; set; }
    public int SalesCount { get; set; }
    public string CategoryName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal RegularPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    public bool Active { get; set; }
    public string ShopId { get; set; } = "";
    public string ShopName { get; set; } = "";
    public string ShopImageUrl { get; set; } = "";

    public ICollection<SubcategoryResponseDto> Subcategories { get; set; } = [];
    public ICollection<ProductPhoto> Photos { get; set; } = [];
}
