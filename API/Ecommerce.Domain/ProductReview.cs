namespace Ecommerce.Domain;

public class ProductReview : BaseEntity
{
    public int Id { get; set; }
    public required int ProductId { get; set; }
    public required string UserId { get; set; }
    public required int Rating { get; set; }
    public string? Review { get; set; }

    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}
