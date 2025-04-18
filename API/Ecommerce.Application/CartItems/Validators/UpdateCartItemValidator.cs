using Ecommerce.Application.CartItems.Commands;
using FluentValidation;

namespace Ecommerce.Application.CartItems.Validators;

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItem.Command>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.UpdateCartItemRequestDto.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required.");

        RuleFor(x => x.UpdateCartItemRequestDto.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity must be greater than or equal to 0.");
    }
}
