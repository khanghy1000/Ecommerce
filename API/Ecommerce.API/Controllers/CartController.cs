using Ecommerce.Application.CartItems.Commands;
using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.CartItems.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemoveFromCart = Ecommerce.Application.CartItems.Commands.RemoveFromCart;

namespace Ecommerce.API.Controllers;

public class CartController : BaseApiController
{
    [HttpGet]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<List<CartItemResponseDto>>> GetCart()
    {
        var result = await Mediator.Send(new GetCartItems.Query());
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<Unit>> AddToCart(AddToCartRequestDto addToCartRequestDto)
    {
        var result = await Mediator.Send(
            new AddToCart.Command { AddToCartRequestDto = addToCartRequestDto }
        );
        return HandleResult(result);
    }

    [HttpPut]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<Unit>> UpdateCartItem(
        UpdateCartItemRequestDto updateCartItemRequestDto
    )
    {
        var result = await Mediator.Send(
            new UpdateCartItem.Command { UpdateCartItemRequestDto = updateCartItemRequestDto }
        );
        return HandleResult(result);
    }

    [HttpDelete]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<Unit>> RemoveFromCart(
        RemoveFromCartRequestDto removeFromCartRequestDto
    )
    {
        var result = await Mediator.Send(
            new RemoveFromCart.Command { RemoveFromCartRequestDto = removeFromCartRequestDto }
        );
        return HandleResult(result);
    }
}
