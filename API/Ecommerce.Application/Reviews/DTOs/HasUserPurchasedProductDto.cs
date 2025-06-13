namespace Ecommerce.Application.Reviews.DTOs;

public class HasUserPurchasedProductDto
{
    public string UserId { get; set; } = null!;
    public int ProductId { get; set; }
    public bool HasPurchased { get; set; }
}
