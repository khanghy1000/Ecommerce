using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Domain;

public class User : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Address { get; set; }
    public int? WardId { get; set; }

    public Ward? Ward { get; set; }
    public ICollection<Product> CreatedProducts { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<ProductReview> Reviews { get; set; } = [];
}
