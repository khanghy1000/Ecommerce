using Ecommerce.Application.Payments.Commands;
using FluentValidation;

namespace Ecommerce.Application.Payments.Validators;

public class CreatePaymentUrlValidator : AbstractValidator<CreatePaymentUrl.Command>
{
    public CreatePaymentUrlValidator()
    {
        RuleFor(x => x.CreatePaymentUrlRequestDto.Money)
            .NotNull()
            .WithMessage("Amount to be paid is required")
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount to be paid must be greater than or equal to 0");
        RuleFor(x => x.CreatePaymentUrlRequestDto.Description)
            .NotEmpty()
            .WithMessage("Transaction description is required");
    }
}
