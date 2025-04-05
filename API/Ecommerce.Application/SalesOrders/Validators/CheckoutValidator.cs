using Ecommerce.Application.SalesOrders.Commands;
using FluentValidation;

namespace Ecommerce.Application.SalesOrders.Validators;

public class CheckoutValidator : AbstractValidator<Checkout.Command>
{
    public CheckoutValidator()
    {
        RuleFor(x => x.CheckoutRequestDto.ShippingName)
            .NotEmpty()
            .WithMessage("Shipping name is required.")
            .MaximumLength(100)
            .WithMessage("Shipping name must not exceed 100 characters.");
        RuleFor(x => x.CheckoutRequestDto.ShippingPhone)
            .NotEmpty()
            .WithMessage("Shipping phone is required.")
            .Matches(@"^\d{10,15}$")
            .WithMessage("Shipping phone must be between 10 and 15 digits.");
        RuleFor(x => x.CheckoutRequestDto.ShippingAddress)
            .NotEmpty()
            .WithMessage("Shipping address is required.")
            .MaximumLength(200)
            .WithMessage("Shipping address must not exceed 200 characters.");
        RuleFor(x => x.CheckoutRequestDto.ShippingWardId)
            .NotNull()
            .WithMessage("Shipping ward ID is required.");
    }
}
