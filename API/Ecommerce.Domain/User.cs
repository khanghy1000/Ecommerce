using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Domain;

public class User : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<Product> CreatedProducts { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<ProductReview> Reviews { get; set; } = [];
    public ICollection<UserAddress> Addresses { get; set; } = [];
}
