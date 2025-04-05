namespace Ecommerce.Application.Categories.DTOs;

public class EditSubcategoryRequestDto
{
    public required string Name { get; set; }
    public int CategoryId { get; set; }
}
