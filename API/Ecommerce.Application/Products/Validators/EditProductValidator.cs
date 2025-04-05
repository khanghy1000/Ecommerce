using Ecommerce.Application.Products.Commands;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class EditProductValidator : AbstractValidator<EditProduct.Command>
{
    public EditProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Product ID is required.");
        RuleFor(x => x.EditProductRequestDto.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(100)
            .WithMessage("Product name must be less than 100 characters.");
        RuleFor(x => x.EditProductRequestDto.Description)
            .NotEmpty()
            .WithMessage("Product description is required.");
        RuleFor(x => x.EditProductRequestDto.RegularPrice)
            .GreaterThan(0)
            .WithMessage("Regular price must be greater than 0.");
        RuleFor(x => x.EditProductRequestDto.DiscountPrice)
            .LessThan(x => x.EditProductRequestDto.RegularPrice)
            .When(x => x.EditProductRequestDto.DiscountPrice.HasValue)
            .WithMessage("Discount price must be less than regular price or null.");
        RuleFor(x => x.EditProductRequestDto.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity must be greater than or equal to 0.");
    }
}
