using Ecommerce.Application.SalesOrders.Commands;
using FluentValidation;

namespace Ecommerce.Application.SalesOrders.Validators;

public class CheckoutPreviewValidator : AbstractValidator<CheckoutPreview.Command>
{
    public CheckoutPreviewValidator()
    {
        RuleFor(x => x.CheckoutPricePreviewRequestDto.ShippingName)
            .NotEmpty()
            .WithMessage("Shipping name is required.")
            .MaximumLength(100)
            .WithMessage("Shipping name must not exceed 100 characters.");
        RuleFor(x => x.CheckoutPricePreviewRequestDto.ShippingPhone)
            .NotEmpty()
            .WithMessage("Shipping phone is required.")
            .Matches(@"^\d{10,15}$")
            .WithMessage("Shipping phone must be between 10 and 15 digits.");
        RuleFor(x => x.CheckoutPricePreviewRequestDto.ShippingAddress)
            .NotEmpty()
            .WithMessage("Shipping address is required.")
            .MaximumLength(200)
            .WithMessage("Shipping address must not exceed 200 characters.");
        RuleFor(x => x.CheckoutPricePreviewRequestDto.ShippingWardId)
            .NotNull()
            .WithMessage("Shipping ward ID is required.");
    }
}
