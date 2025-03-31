using Ecommerce.Application.Categories.Commands;
using FluentValidation;

namespace Ecommerce.Application.Categories.Validators;

public class EditCategoryCommandValidator : AbstractValidator<EditCategory.Command>
{
    public EditCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Category ID is required.");
        RuleFor(x => x.CategoryDto.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(100)
            .WithMessage("Category name should not exceed 100 characters.");
    }
}
