using Ecommerce.Application.Products.Commands;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class CreateProductValidator : AbstractValidator<CreateProduct.Command>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.CreateProductRequestDto.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(100)
            .WithMessage("Product name must be less than 100 characters.");
        RuleFor(x => x.CreateProductRequestDto.Description)
            .NotEmpty()
            .WithMessage("Product description is required.");
        RuleFor(x => x.CreateProductRequestDto.RegularPrice)
            .GreaterThan(0)
            .WithMessage("Regular price must be greater than 0.");
        RuleFor(x => x.CreateProductRequestDto.DiscountPrice)
            .LessThan(x => x.CreateProductRequestDto.RegularPrice)
            .When(x => x.CreateProductRequestDto.DiscountPrice.HasValue)
            .WithMessage("Discount price must be less than regular price or null.");
        RuleFor(x => x.CreateProductRequestDto.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity must be greater than or equal to 0.");
    }
}
