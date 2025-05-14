using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Statistics.DTOs;
using Ecommerce.Domain;
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

            var avgRating = await dbContext
                .ProductReviews.Where(r => r.Product.ShopId == request.ShopId)
                .Select(r => (decimal)r.Rating)
                .DefaultIfEmpty(0)
                .AverageAsync(cancellationToken);

            var result = new ShopOrderStatsResponseDto
            {
                TotalOrders = totalOrders,
                AverageRating = avgRating,
            };

            return Result<ShopOrderStatsResponseDto>.Success(result);
        }
    }
}
