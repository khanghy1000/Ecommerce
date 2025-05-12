using System.Text.RegularExpressions;
using Ecommerce.Application.Users.Commands;
using FluentValidation;

namespace Ecommerce.Application.Users.Validators;

public class AddUserAddressValidator : AbstractValidator<AddUserAddress.Command>
{
    public AddUserAddressValidator()
    {
        RuleFor(x => x.AddUserAddressRequestDto.Address)
            .NotEmpty()
            .WithMessage("Address is required")
            .MaximumLength(200)
            .WithMessage("Address must be less than 200 characters");

        RuleFor(x => x.AddUserAddressRequestDto.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must be less than 100 characters");

        RuleFor(x => x.AddUserAddressRequestDto.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .MinimumLength(10)
            .WithMessage("Phone Number must not be less than 10 characters.")
            .MaximumLength(20)
            .WithMessage("Phone Number must not exceed 50 characters.");

        RuleFor(x => x.AddUserAddressRequestDto.WardId)
            .NotEmpty()
            .WithMessage("Ward id is required");
    }
}
