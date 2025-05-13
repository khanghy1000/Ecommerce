using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public static class GetProductDiscountById
{
    public class Query : IRequest<Result<ProductDiscountResponseDto>>
    {
        public int ProductId { get; set; }
        public int DiscountId { get; set; }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.DiscountId).NotEmpty();
        }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<ProductDiscountResponseDto>>
    {
        public async Task<Result<ProductDiscountResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var discount = await dbContext
                .ProductDiscounts.Include(d => d.Product)
                .FirstOrDefaultAsync(
                    d => d.Id == request.DiscountId && d.ProductId == request.ProductId,
                    cancellationToken
                );

            if (discount == null)
                return Result<ProductDiscountResponseDto>.Failure("Discount not found", 404);

            var discountDto = mapper.Map<ProductDiscountResponseDto>(discount);

            return Result<ProductDiscountResponseDto>.Success(discountDto);
        }
    }
}
