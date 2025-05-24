using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Statistics.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Statistics.Queries;

public static class GetShopSummary
{
    public class Query : IRequest<Result<ShopOrderStatsResponseDto>>
    {
        public required string ShopId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<ShopOrderStatsResponseDto>>
    {
        public async Task<Result<ShopOrderStatsResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var totalOrders = await dbContext
                .SalesOrders.Where(o =>
                    o.OrderProducts.Any(op => op.Product.ShopId == request.ShopId)
                )
                .CountAsync(cancellationToken);

            decimal avgRating = 0;
            var hasReviews = await dbContext.ProductReviews
                .AnyAsync(r => r.Product.ShopId == request.ShopId, cancellationToken);

            if (hasReviews)
            {
                avgRating =(decimal) await dbContext.ProductReviews
                    .Where(r => r.Product.ShopId == request.ShopId)
                    .Select(r => r.Rating)
                    .AverageAsync(cancellationToken);
            }

            var result = new ShopOrderStatsResponseDto
            {
                TotalOrders = totalOrders,
                AverageRating = avgRating,
            };

            return Result<ShopOrderStatsResponseDto>.Success(result);
        }
    }
}