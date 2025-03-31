using Ecommerce.Application.Products.Commands;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class DeleteProductPhotoValidator : AbstractValidator<DeleteProductPhoto.Command>
{
    public DeleteProductPhotoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Key).NotEmpty().WithMessage("Photo key is required");
    }
}
