namespace Ecommerce.Application.Categories.DTOs;

public class SubcategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
}
