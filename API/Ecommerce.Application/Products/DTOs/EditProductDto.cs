using Ecommerce.Domain;

namespace Ecommerce.Application.Products.DTOs;

public class EditProductDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal RegularPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    public bool Active { get; set; }
    public List<int> SubcategoryIds { get; set; } = [];
}
