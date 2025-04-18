﻿namespace Ecommerce.Domain;

public class SalesOrder : BaseEntity
{
    public int Id { get; set; }
    public DateTime OrderTime { get; set; } = DateTime.UtcNow;
    public required string UserId { get; set; }
    public int? CouponId { get; set; }
    public required decimal Subtotal { get; set; }
    public required int ShippingFee { get; set; }
    public required decimal Total { get; set; }
    public string? ShippingOrderCode { get; set; }
    public required string ShippingName { get; set; }
    public required string ShippingPhone { get; set; }
    public required string ShippingAddress { get; set; }
    public required int ShippingWardId { get; set; }
    public required PaymentMethod PaymentMethod { get; set; }
    public required SalesOrderStatus Status { get; set; }

    public User User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public ICollection<OrderProduct> OrderProducts { get; set; } = [];
    public Ward ShippingWard { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = [];
}
