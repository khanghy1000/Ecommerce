namespace Ecommerce.Application.Products.DTOs;

public class UpdateProductPhotoDisplayOrderDto
{
    public required string Key { get; set; }
    public required int DisplayOrder { get; set; }
}
