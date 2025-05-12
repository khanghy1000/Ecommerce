using Ecommerce.Domain;

namespace Ecommerce.Application.Coupons.DTOs;

public class EditCouponRequestDto
{
    public bool Active { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public CouponType Type { get; set; }
    public CouponDiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal MaxDiscountAmount { get; set; }
    public bool AllowMultipleUse { get; set; }
    public int MaxUseCount { get; set; }
    public List<int>? CategoryIds { get; set; }
}
