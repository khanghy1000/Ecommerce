using Ecommerce.Application.Categories.Commands;
using FluentValidation;

namespace Ecommerce.Application.Categories.Validators;

public class EditCategoryCommandValidator : AbstractValidator<EditCategory.Command>
{
    public EditCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CategoryDto.Name).NotEmpty().MaximumLength(100);
    }
}
