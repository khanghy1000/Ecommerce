using Ecommerce.Application.Products.Commands;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class EditProductValidator : AbstractValidator<EditProduct.Command>
{
    public EditProductValidator()
    {
        RuleFor(x => x.ProductDto.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProductDto.Description).NotEmpty();
        RuleFor(x => x.ProductDto.RegularPrice).NotEmpty().GreaterThan(0);
        RuleFor(x => x.ProductDto.DiscountPrice)
            .LessThan(x => x.ProductDto.RegularPrice)
            .When(x => x.ProductDto.DiscountPrice.HasValue);
        RuleFor(x => x.ProductDto.Quantity).NotEmpty().GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProductDto.ProductStatus).IsInEnum();
    }
}
