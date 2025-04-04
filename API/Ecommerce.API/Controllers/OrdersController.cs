using Ecommerce.Application.SalesOrders.Commands;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class OrdersController : BaseApiController
{
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout(
        [FromBody] CheckoutDto checkoutDto
    )
    {
        var result = await Mediator.Send(new Checkout.Command { CheckoutDto = checkoutDto });
        return HandleResult(result);
    }

    [HttpPost("checkout-review")]
    public async Task<ActionResult<CheckoutPriceReviewResponseDto>> CheckoutReview(
        [FromBody] CheckoutPriceReviewDto priceReviewDto
    )
    {
        var result = await Mediator.Send(
            new CheckoutReview.Command { PriceReviewDto = priceReviewDto }
        );
        return HandleResult(result);
    }
}
