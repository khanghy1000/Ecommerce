using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Coupons.Queries;

public static class ListValidCoupons
{
    public class Query : IRequest<Result<List<CouponResponseDto>>>
    {
        public decimal OrderSubtotal { get; set; }
        public List<int> ProductCategoryIds { get; set; } = [];
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<List<CouponResponseDto>>>
    {
        public async Task<Result<List<CouponResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var currentTime = DateTime.UtcNow;

            var user = await userAccessor.GetUserAsync();

            var usedCouponCodes = await dbContext
                .SalesOrders.Where(o => o.UserId == user.Id && o.Coupons.Count > 0)
                .SelectMany(o => o.Coupons.Select(c => c.Code))
                .Distinct()
                .ToListAsync(cancellationToken);

            var validCouponsQuery = dbContext
                .Coupons.Include(c => c.Categories)
                .Where(c =>
                    c.Active
                    && c.StartTime <= currentTime
                    && c.EndTime >= currentTime
                    && c.UsedCount < c.MaxUseCount
                    && c.MinOrderValue <= request.OrderSubtotal
                    && (c.AllowMultipleUse || !usedCouponCodes.Contains(c.Code))
                );

            if (request.ProductCategoryIds.Count > 0)
            {
                validCouponsQuery = validCouponsQuery.Where(c =>
                    c.Categories.Count == 0
                    || c.Categories.Any(cc => request.ProductCategoryIds.Contains(cc.Id))
                );
            }

            var validCoupons = await validCouponsQuery
                .ProjectTo<CouponResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<CouponResponseDto>>.Success(validCoupons);
        }
    }
}
