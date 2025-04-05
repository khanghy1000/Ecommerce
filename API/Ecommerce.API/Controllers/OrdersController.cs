using Ecommerce.Application.SalesOrders.Commands;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class OrdersController : BaseApiController
{
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout(
        [FromBody] CheckoutRequestDto checkoutRequestDto
    )
    {
        var result = await Mediator.Send(
            new Checkout.Command { CheckoutRequestDto = checkoutRequestDto }
        );
        return HandleResult(result);
    }

    [HttpPost("checkout-review")]
    public async Task<ActionResult<CheckoutPriceReviewResponseDto>> CheckoutReview(
        [FromBody] CheckoutPriceReviewRequestDto checkoutPriceReviewRequestDto
    )
    {
        var result = await Mediator.Send(
            new CheckoutReview.Command
            {
                CheckoutPriceReviewRequestDto = checkoutPriceReviewRequestDto,
            }
        );
        return HandleResult(result);
    }
}
