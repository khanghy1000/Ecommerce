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
                    TimeRange.Days => DateTime.UtcNow.AddDays(-request.TimeValue.Value),
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

            // Define end date as current time
            DateTime endDate = DateTime.UtcNow;

            // If no startDate was specified (TimeRange.All), find the earliest order
            if (!startDate.HasValue && orders.Any())
            {
                startDate = orders.Min(o => o.OrderTime);
                // For "All" time range, default to monthly grouping if not empty
                if (request.TimeRange == TimeRange.All)
                {
                    // Round down to first day of month
                    startDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                }
            }
            else if (!startDate.HasValue)
            {
                // If no orders and no startDate specified, default to 6 months ago
                startDate = DateTime.UtcNow.AddMonths(-6);
                if (request.TimeRange == TimeRange.All)
                {
                    startDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                }
            }

            // Generate all time periods that should exist in the result
            var allTimePeriods = new Dictionary<DateTime, ShopPerformanceResponseDto>();

            switch (request.TimeRange)
            {
                case TimeRange.Days:
                    for (
                        var date = startDate.Value.Date;
                        date <= endDate.Date;
                        date = date.AddDays(1)
                    )
                    {
                        allTimePeriods[date] = new ShopPerformanceResponseDto
                        {
                            Time = date,
                            Quantity = 0,
                            Value = 0,
                            OrderCount = 0,
                        };
                    }
                    break;

                case TimeRange.Months:
                    var monthStart = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                    var monthEnd = new DateTime(endDate.Year, endDate.Month, 1);
                    for (var date = monthStart; date <= monthEnd; date = date.AddMonths(1))
                    {
                        allTimePeriods[date] = new ShopPerformanceResponseDto
                        {
                            Time = date,
                            Quantity = 0,
                            Value = 0,
                            OrderCount = 0,
                        };
                    }
                    break;

                case TimeRange.Years:
                    var yearStart = new DateTime(startDate.Value.Year, 1, 1);
                    var yearEnd = new DateTime(endDate.Year, 1, 1);
                    for (var date = yearStart; date <= yearEnd; date = date.AddYears(1))
                    {
                        allTimePeriods[date] = new ShopPerformanceResponseDto
                        {
                            Time = date,
                            Quantity = 0,
                            Value = 0,
                            OrderCount = 0,
                        };
                    }
                    break;

                default: // TimeRange.All - use monthly by default
                    var allMonthStart = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                    var allMonthEnd = new DateTime(endDate.Year, endDate.Month, 1);
                    for (var date = allMonthStart; date <= allMonthEnd; date = date.AddMonths(1))
                    {
                        allTimePeriods[date] = new ShopPerformanceResponseDto
                        {
                            Time = date,
                            Quantity = 0,
                            Value = 0,
                            OrderCount = 0,
                        };
                    }
                    break;
            }

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
                    // case TimeRange.All:  group by month
                    groupedOrders = orders.GroupBy(o => new DateTime(
                        o.OrderTime.Year,
                        o.OrderTime.Month,
                        1
                    ));
                    break;
            }

            // Fill in actual data where it exists
            foreach (var group in groupedOrders)
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

                // Update the existing entry in our dictionary
                if (allTimePeriods.ContainsKey(group.Key))
                {
                    allTimePeriods[group.Key] = new ShopPerformanceResponseDto
                    {
                        Time = group.Key,
                        Quantity = totalQuantity,
                        Value = totalValue,
                        OrderCount = orderCount,
                    };
                }
            }

            // Convert dictionary values to list and sort by time
            performanceData = allTimePeriods.Values.OrderBy(d => d.Time).ToList();

            return Result<List<ShopPerformanceResponseDto>>.Success(performanceData);
        }
    }
}
