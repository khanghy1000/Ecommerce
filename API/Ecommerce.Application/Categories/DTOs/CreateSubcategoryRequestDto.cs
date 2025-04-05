namespace Ecommerce.Application.Categories.DTOs;

public class CreateSubcategoryRequestDto
{
    public required string Name { get; set; }
    public int CategoryId { get; set; }
}
