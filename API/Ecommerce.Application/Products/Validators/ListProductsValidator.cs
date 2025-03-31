using Ecommerce.Application.Products.Queries;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class ListProductsValidator : AbstractValidator<ListProducts.Query>
{
    public ListProductsValidator()
    {
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("Page size must be between 1 and 50.");
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.SortBy)
            .Must(x => x is "name" or "price")
            .WithMessage("Sort by must be either 'name' or 'price'.");
        RuleFor(x => x.SortDirection)
            .Must(x => x is "asc" or "desc")
            .WithMessage("Sort direction must be either 'asc' or 'desc'.");
    }
}
