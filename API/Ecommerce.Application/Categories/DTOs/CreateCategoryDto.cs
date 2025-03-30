namespace Ecommerce.Application.Categories.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; }
    public int? ParentId { get; set; }
}
