using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Application.SalesOrders.Queries;
using FluentValidation;

namespace Ecommerce.Application.SalesOrders.Validators;

public class ListSalesOrderValidator : AbstractValidator<ListSalesOrders.Query>
{
    public ListSalesOrderValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than 0");
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50");

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("From date must be less than or equal to To date");
    }
}
