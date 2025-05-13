using Ecommerce.Application.Products.DTOs;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class AddProductDiscountValidator : AbstractValidator<AddProductDiscountRequestDto>
{
    public AddProductDiscountValidator()
    {
        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0)
            .WithMessage("Discount price must be greater than zero");

        RuleFor(x => x.StartTime).NotEmpty().WithMessage("Start time is required");

        RuleFor(x => x.EndTime).NotEmpty().WithMessage("End time is required");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time");
    }
}
