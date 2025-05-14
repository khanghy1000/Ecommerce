using Ecommerce.Application.Reviews.DTOs;
using FluentValidation;

namespace Ecommerce.Application.Reviews.Validators;

public class CreateReviewValidator : AbstractValidator<CreateReviewRequestDto>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Rating)
            .NotEmpty()
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");
        RuleFor(x => x.Review)
            .MaximumLength(1000)
            .WithMessage("Review cannot exceed 1000 characters");
    }
}
