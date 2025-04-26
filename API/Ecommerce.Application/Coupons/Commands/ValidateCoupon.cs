using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Coupons.Commands;

public class ValidateCoupon
{
    public class Command : IRequest<Result<Coupon>>
    {
        public required string CouponCode { get; set; }
        public CouponType CouponType { get; set; }
        public decimal OrderSubtotal { get; set; }
        public List<int> ProductCategoryIds { get; set; } = [];
    }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Coupon>>
    {
        public async Task<Result<Coupon>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var coupon = await dbContext
                .Coupons.Include(c => c.Categories)
                .FirstOrDefaultAsync(
                    c =>
                        c.Code == request.CouponCode
                        && c.Active
                        && c.StartTime <= DateTime.UtcNow
                        && c.EndTime >= DateTime.UtcNow
                        && c.Type == request.CouponType,
                    cancellationToken
                );

            if (coupon == null)
            {
                return Result<Coupon>.Failure("Invalid or expired coupon", 400);
            }

            if (!coupon.AllowMultipleUse)
            {
                var user = await userAccessor.GetUserAsync();
                var userUsedCoupon = await dbContext.SalesOrders.AnyAsync(
                    o =>
                        o.UserId == user.Id
                        && o.Coupons.Count >= 1
                        && o.Coupons.Any(c => c.Code == request.CouponCode),
                    cancellationToken
                );

                if (userUsedCoupon)
                {
                    return Result<Coupon>.Failure("You have already used this coupon", 400);
                }
            }

            if (coupon.UsedCount >= coupon.MaxUseCount)
            {
                return Result<Coupon>.Failure("Coupon usage limit reached", 400);
            }

            if (request.OrderSubtotal < coupon.MinOrderValue)
            {
                return Result<Coupon>.Failure(
                    $"Order subtotal must be at least {coupon.MinOrderValue}",
                    400
                );
            }

            if (coupon.Categories.Count == 0)
                return Result<Coupon>.Success(coupon);

            var hasCategoryMatch = coupon.Categories.Any(cc =>
                request.ProductCategoryIds.Contains(cc.Id)
            );

            if (!hasCategoryMatch)
            {
                return Result<Coupon>.Failure(
                    "Coupon is not applicable to the selected products",
                    400
                );
            }

            return Result<Coupon>.Success(coupon);
        }
    }
}
