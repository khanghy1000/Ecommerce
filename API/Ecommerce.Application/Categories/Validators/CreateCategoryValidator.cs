using Ecommerce.Application.Categories.Commands;
using FluentValidation;

namespace Ecommerce.Application.Categories.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategory.Command>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.CategoryDto.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(100)
            .WithMessage("Category name should not exceed 100 characters.");
    }
}
