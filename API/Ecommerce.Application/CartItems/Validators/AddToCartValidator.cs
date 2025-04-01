using Ecommerce.Application.CartItems.Commands;
using FluentValidation;

namespace Ecommerce.Application.CartItems.Validators;

public class AddToCartValidator : AbstractValidator<AddToCart.Command>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ItemDto.ProductId).NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.ItemDto.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}
