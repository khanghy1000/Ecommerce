namespace Ecommerce.Application.Categories.DTOs;

public class SubcategoryResponseDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
}
