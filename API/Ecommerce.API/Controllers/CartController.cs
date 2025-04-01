using Ecommerce.Application.CartItems.Commands;
using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.CartItems.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RemoveFromCart = Ecommerce.Application.CartItems.Commands.RemoveFromCart;

namespace Ecommerce.API.Controllers;

public class CartController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<CartItemDto>>> GetCart()
    {
        var result = await Mediator.Send(new GetCartItems.Query());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> AddToCart(AddToCartDto itemDto)
    {
        var result = await Mediator.Send(new AddToCart.Command { ItemDto = itemDto });
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<ActionResult<Unit>> UpdateCartItem(UpdateCartItemDto itemDto)
    {
        var result = await Mediator.Send(new UpdateCartItem.Command { ItemDto = itemDto });
        return HandleResult(result);
    }

    [HttpDelete]
    public async Task<ActionResult<Unit>> RemoveFromCart(RemoveFromCartDto itemDto)
    {
        var result = await Mediator.Send(new RemoveFromCart.Command { ItemDto = itemDto });
        return HandleResult(result);
    }
}
