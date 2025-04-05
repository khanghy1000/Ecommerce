namespace Ecommerce.Application.Products.DTOs;

public class UpdateProductPhotoDisplayOrderRequestDto
{
    public required string Key { get; set; }
    public required int DisplayOrder { get; set; }
}
