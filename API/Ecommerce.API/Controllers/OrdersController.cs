using Ecommerce.Application.Core;
using Ecommerce.Application.SalesOrders.Commands;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Application.SalesOrders.Queries;
using Ecommerce.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class OrdersController : BaseApiController
{
    [HttpPost("checkout")]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout(
        [FromBody] CheckoutRequestDto checkoutRequestDto
    )
    {
        var result = await Mediator.Send(
            new Checkout.Command { CheckoutRequestDto = checkoutRequestDto }
        );
        return HandleResult(result);
    }

    [HttpPost("checkout-preview")]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<CheckoutPricePreviewResponseDto>> CheckoutReview(
        [FromBody] CheckoutPricePreviewRequestDto checkoutPricePreviewRequestDto
    )
    {
        var result = await Mediator.Send(
            new CheckoutPreview.Command
            {
                CheckoutPricePreviewRequestDto = checkoutPricePreviewRequestDto,
            }
        );
        return HandleResult(result);
    }

    [HttpGet()]
    [Authorize]
    public async Task<ActionResult<PagedList<SalesOrderResponseDto>>> ListSalesOrders(
        [FromQuery] int? orderId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] SalesOrderStatus? status,
        [FromQuery] string? buyerId,
        [FromQuery] string? shopId,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1
    )
    {
        var salesOrders = await Mediator.Send(
            new ListSalesOrders.Query
            {
                OrderId = orderId,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                BuyerId = buyerId,
                ShopId = shopId,
                PageSize = pageSize,
                PageNumber = pageNumber,
            }
        );
        return HandleResult(salesOrders);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "HasOrder")]
    public async Task<ActionResult<SalesOrderResponseDto>> GetSalesOrderById(int id)
    {
        var salesOrder = await Mediator.Send(new GetSalesOrderById.Query { Id = id });
        return HandleResult(salesOrder);
    }
}
