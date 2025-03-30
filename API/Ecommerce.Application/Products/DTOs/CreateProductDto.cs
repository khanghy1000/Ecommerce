using Ecommerce.Domain;

namespace Ecommerce.Application.Products.DTOs;

public class CreateProductDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal RegularPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    public ProductStatus ProductStatus { get; set; }
    public List<int> CategoryIds { get; set; } = [];
    public List<int> TagIds { get; set; } = [];
}
