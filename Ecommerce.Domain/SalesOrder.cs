namespace Ecommerce.Domain;

public class SalesOrder : BaseEntity
{
    public int Id { get; set; }
    public required DateTime OrderTime { get; set; } = DateTime.UtcNow;
    public required decimal Total { get; set; }
    public required string UserId { get; set; }
    public int? CouponId { get; set; }
    
    public User User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public ICollection<OrderProduct> OrderProducts { get; set; } = [];
}