using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Application.Statistics.DTOs;
using Ecommerce.Application.Statistics.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[AllowAnonymous]
public class StatsController : BaseApiController
{
    [HttpGet("shop-performance")]
    public async Task<ActionResult<List<ShopPerformanceResponseDto>>> ShopPerformance(
        string shopId,
        GetShopPerformance.MetricType metricType = GetShopPerformance.MetricType.Orders,
        GetShopPerformance.TimeRange timeRange = GetShopPerformance.TimeRange.All,
        int? timeValue = null // number of days/months/years
    )
    {
        var result = await Mediator.Send(
            new GetShopPerformance.Query
            {
                ShopId = shopId,
                TimeRange = timeRange,
                TimeValue = timeValue,
            }
        );

        return HandleResult(result);
    }

    [HttpGet("shop-summary")]
    public async Task<ActionResult<ShopOrderStatsResponseDto>> ShopSummary(string shopId)
    {
        var result = await Mediator.Send(new GetShopSummary.Query { ShopId = shopId });
        return HandleResult(result);
    }
}
