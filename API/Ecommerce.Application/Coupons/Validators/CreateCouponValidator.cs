using Ecommerce.Application.Coupons.Commands;
using FluentValidation;

namespace Ecommerce.Application.Coupons.Validators;

public class CreateCouponValidator : AbstractValidator<CreateCoupon.Command>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.CouponRequest.Code).NotEmpty().WithMessage("Coupon code is required");
        RuleFor(x => x.CouponRequest.StartTime).NotEmpty().WithMessage("Start time is required");
        RuleFor(x => x.CouponRequest.EndTime).NotEmpty().WithMessage("End time is required");
        RuleFor(x => x.CouponRequest.EndTime)
            .GreaterThan(x => x.CouponRequest.StartTime)
            .WithMessage("End time must be after start time");
        RuleFor(x => x.CouponRequest.Value)
            .GreaterThan(0)
            .WithMessage("Value must be greater than 0");
        RuleFor(x => x.CouponRequest.MinOrderValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum order value must be greater than or equal to 0");
        RuleFor(x => x.CouponRequest.MaxDiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum discount amount must be greater than or equal to 0");
        RuleFor(x => x.CouponRequest.MaxUseCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum use count must be greater than or equal to 0");
        RuleFor(x => x.CouponRequest.Type).IsInEnum().WithMessage("Invalid coupon type");
        RuleFor(x => x.CouponRequest.DiscountType).IsInEnum().WithMessage("Invalid discount type");

        // If discount type is percent, value must be between 0 and 100
        RuleFor(x => x.CouponRequest.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.CouponRequest.DiscountType == Domain.CouponDiscountType.Percent)
            .WithMessage("Percentage value must be between 0 and 100");
    }
}
