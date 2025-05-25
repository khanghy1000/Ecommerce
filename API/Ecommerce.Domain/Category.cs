namespace Ecommerce.Domain;

public class Category : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Subcategory> Subcategories { get; set; } = [];
    public ICollection<Coupon> Coupons { get; set; } = [];
}
