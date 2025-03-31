using Ecommerce.Application.Products.Commands;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class UpdateProductPhotoDisplayOrderValidator
    : AbstractValidator<UpdateProductPhotoDisplayOrder.Command>
{
    public UpdateProductPhotoDisplayOrderValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.PhotoOrders)
            .NotNull()
            .WithMessage("Photo orders list cannot be null")
            .Must(list => list.Count > 0)
            .WithMessage("At least one photo order must be specified");

        RuleForEach(x => x.PhotoOrders)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.Key).NotEmpty().WithMessage("Photo key is required");

                item.RuleFor(x => x.DisplayOrder)
                    .GreaterThan(0)
                    .WithMessage("Display order must be greater than 0");
            });

        RuleFor(x => x.PhotoOrders)
            .Must(list => list.Select(i => i.Key).Distinct().Count() == list.Count)
            .WithMessage("Duplicate photo keys are not allowed");

        RuleFor(x => x.PhotoOrders)
            .Must(list => list.Select(i => i.DisplayOrder).Distinct().Count() == list.Count)
            .WithMessage("Duplicate display orders are not allowed");
    }
}
