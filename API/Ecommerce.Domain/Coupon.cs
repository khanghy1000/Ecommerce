using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Domain;

public class Coupon : BaseEntity
{
    [Key]
    public required string Code { get; set; }
    public required bool Active { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required CouponType Type { get; set; }
    public required CouponDiscountType DiscountType { get; set; }
    public required decimal Value { get; set; }
    public required decimal MinOrderValue { get; set; }
    public required decimal MaxDiscountAmount { get; set; }
    public required bool AllowMultipleUse { get; set; }
    public required int MaxUseCount { get; set; }
    public required int UsedCount { get; set; }

    public ICollection<SalesOrder> SalesOrders { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];
}

public enum CouponType
{
    Product,
    Shipping,
}

public enum CouponDiscountType
{
    Percent,
    Amount,
}
