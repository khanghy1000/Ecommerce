using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public static class ListProductDiscounts
{
    public class Query : IRequest<Result<List<ProductDiscountResponseDto>>>
    {
        public int ProductId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<ProductDiscountResponseDto>>>
    {
        public async Task<Result<List<ProductDiscountResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.ProductId,
                cancellationToken
            );

            if (product == null)
                return Result<List<ProductDiscountResponseDto>>.Failure("Product not found", 404);

            var discounts = await dbContext
                .ProductDiscounts.Where(d => d.ProductId == request.ProductId)
                .OrderBy(d => d.StartTime)
                .ThenByDescending(d => d.DiscountPrice)
                .ProjectTo<ProductDiscountResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<ProductDiscountResponseDto>>.Success(discounts);
        }
    }
}
