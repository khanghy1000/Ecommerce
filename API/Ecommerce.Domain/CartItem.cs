namespace Ecommerce.Domain;

public class CartItem
{
    public required string UserId { get; set; }
    public required int ProductId { get; set; }
    public required int Quantity { get; set; }

    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}