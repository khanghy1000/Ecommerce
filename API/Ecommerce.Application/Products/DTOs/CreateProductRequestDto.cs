using Ecommerce.Domain;

namespace Ecommerce.Application.Products.DTOs;

public class CreateProductRequestDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal RegularPrice { get; set; }
    public int Quantity { get; set; }
    public bool Active { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Weight { get; set; }
    public List<int> SubcategoryIds { get; set; } = [];
}
