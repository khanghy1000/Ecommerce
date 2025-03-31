using Ecommerce.Application.Products.Commands;
using FluentValidation;

namespace Ecommerce.Application.Products.Validators;

public class AddProductPhotoValidator : AbstractValidator<AddProductPhoto.Command>
{
    public AddProductPhotoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.File).NotNull().WithMessage("Photo file is required");

        RuleFor(x => x.File)
            .Must(file => file != null && ValidImageTypes.Contains(file.ContentType.ToLower()))
            .WithMessage("File must be a valid image (jpg, jpeg, png, gif, webp)");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(10_000_000) // 10MB
            .WithMessage("File size must be less than 10MB");
    }

    private static readonly string[] ValidImageTypes =
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
    };
}
