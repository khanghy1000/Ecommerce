using Ecommerce.Application.Products.Commands;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class CreateProductValidator : AbstractValidator<CreateProduct.Command>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.ProductDto.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(100)
            .WithMessage("Product name must be less than 100 characters.");
        RuleFor(x => x.ProductDto.Description)
            .NotEmpty()
            .WithMessage("Product description is required.");
        RuleFor(x => x.ProductDto.RegularPrice)
            .GreaterThan(0)
            .WithMessage("Regular price must be greater than 0.");
        RuleFor(x => x.ProductDto.DiscountPrice)
            .LessThan(x => x.ProductDto.RegularPrice)
            .When(x => x.ProductDto.DiscountPrice.HasValue)
            .WithMessage("Discount price must be less than regular price or null.");
        RuleFor(x => x.ProductDto.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity must be greater than or equal to 0.");
    }
}
