namespace Ecommerce.Application.Categories.DTOs;

public class CreateSubcategoryDto
{
    public required string Name { get; set; }
    public int CategoryId { get; set; }
}
