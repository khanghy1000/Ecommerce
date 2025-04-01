using Ecommerce.Application.Categories.Commands;
using FluentValidation;

namespace Ecommerce.Application.Categories.Validators;

public class DeleteSubcategoryValidator : AbstractValidator<DeleteSubcategory.Command>
{
    public DeleteSubcategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Subcategory ID is required.");
    }
}
