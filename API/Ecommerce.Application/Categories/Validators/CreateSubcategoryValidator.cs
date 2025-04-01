using Ecommerce.Application.Categories.Commands;
using FluentValidation;

namespace Ecommerce.Application.Categories.Validators;

public class CreateSubcategoryValidator : AbstractValidator<CreateSubcategory.Command>
{
    public CreateSubcategoryValidator()
    {
        RuleFor(x => x.SubcategoryDto.Name)
            .NotEmpty()
            .WithMessage("Subcategory name is required.")
            .MaximumLength(100)
            .WithMessage("Subcategory name should not exceed 100 characters.");

        RuleFor(x => x.SubcategoryDto.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID is required.");
    }
}
