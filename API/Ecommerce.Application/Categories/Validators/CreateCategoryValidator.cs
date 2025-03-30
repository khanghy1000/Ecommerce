using Ecommerce.Application.Categories.Commands;
using FluentValidation;

namespace Ecommerce.Application.Categories.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategory.Command>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.CategoryDto.Name).NotEmpty().MaximumLength(100);
    }
}
