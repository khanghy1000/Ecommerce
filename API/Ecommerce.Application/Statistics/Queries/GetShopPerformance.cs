using System;
using System.Collections.Generic;
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

public static class GetShopPerformance
{
    public enum MetricType
    {
        Quantity,
        Value,
        Orders,
    }

    public enum TimeRange
    {
        Days,
        Months,
        Years,
        All,
    }

    public class Query : IRequest<Result<List<ShopPerformanceResponseDto>>>
    {
        public required string ShopId { get; set; }
        public TimeRange TimeRange { get; set; } = TimeRange.All;
        public int? TimeValue { get; set; }
    }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<List<ShopPerformanceResponseDto>>>
    {
        public async Task<Result<List<ShopPerformanceResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            DateTime? startDate = null;
            if (request.TimeRange != TimeRange.All && request.TimeValue.HasValue)
            {
                startDate = request.TimeRange switch
                {
                    TimeRange.All => DateTime.UtcNow.AddDays(-request.TimeValue.Value),
                    TimeRange.Months => DateTime.UtcNow.AddMonths(-request.TimeValue.Value),
                    TimeRange.Years => DateTime.UtcNow.AddYears(-request.TimeValue.Value),
                    _ => null,
                };
            }

            // Get orders for the shop
            var query = dbContext
                .SalesOrders.Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.OrderProducts.Any(op => op.Product.ShopId == request.ShopId))
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderTime >= startDate.Value);
            }

            var orders = await query.ToListAsync(cancellationToken);

            var performanceData = new List<ShopPerformanceResponseDto>();

            // Group data by time period
            IEnumerable<IGrouping<DateTime, SalesOrder>> groupedOrders;

            switch (request.TimeRange)
            {
                case TimeRange.Days:
                    groupedOrders = orders.GroupBy(o => o.OrderTime.Date);
                    break;
                case TimeRange.Months:
                    groupedOrders = orders.GroupBy(o => new DateTime(
                        o.OrderTime.Year,
                        o.OrderTime.Month,
                        1
                    ));
                    break;
                case TimeRange.Years:
                    groupedOrders = orders.GroupBy(o => new DateTime(o.OrderTime.Year, 1, 1));
                    break;
                default:
                    // case TimeRange.All:
                    // group by month to show historical trend
                    groupedOrders = orders.GroupBy(o => new DateTime(
                        o.OrderTime.Year,
                        o.OrderTime.Month,
                        1
                    ));
                    break;
            }

            foreach (var group in groupedOrders.OrderBy(g => g.Key))
            {
                int orderCount = group.Count();
                int totalQuantity = 0;
                decimal totalValue = 0;

                foreach (var order in group)
                {
                    // An order only contains products from the same shop
                    // We filter products by shopId here just to be safe
                    var shopOrderProducts = order
                        .OrderProducts.Where(op => op.Product.ShopId == request.ShopId)
                        .ToList();
                    totalQuantity += shopOrderProducts.Sum(op => op.Quantity);
                    totalValue += shopOrderProducts.Sum(op => op.Subtotal);
                }

                performanceData.Add(
                    new ShopPerformanceResponseDto
                    {
                        Time = group.Key,
                        Quantity = totalQuantity,
                        Value = totalValue,
                        OrderCount = orderCount,
                    }
                );
            }

            return Result<List<ShopPerformanceResponseDto>>.Success(performanceData);
        }
    }
}
