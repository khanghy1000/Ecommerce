using Ecommerce.Application.SalesOrders.Commands;
using Ecommerce.Application.SalesOrders.DTOs;
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
}
