using Ecommerce.Application.Coupons.Commands;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Application.Coupons.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class CouponsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<CouponResponseDto>>> GetCoupons()
    {
        return HandleResult(await Mediator.Send(new ListCoupons.Query()));
    }

    [HttpGet("valid")]
    public async Task<ActionResult<List<CouponResponseDto>>> GetValidCoupons(
        [FromQuery] decimal orderSubtotal,
        [FromQuery] List<int> productCategoryIds
    )
    {
        return HandleResult(
            await Mediator.Send(
                new ListValidCoupons.Query
                {
                    OrderSubtotal = orderSubtotal,
                    ProductCategoryIds = productCategoryIds,
                }
            )
        );
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CouponResponseDto>> CreateCoupon(
        CreateCouponRequestDto couponRequest
    )
    {
        return HandleResult(
            await Mediator.Send(new CreateCoupon.Command { CouponRequest = couponRequest })
        );
    }

    [HttpPut("{code}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CouponResponseDto>> EditCoupon(
        string code,
        EditCouponRequestDto couponRequest
    )
    {
        return HandleResult(
            await Mediator.Send(
                new EditCoupon.Command { Code = code, CouponRequest = couponRequest }
            )
        );
    }

    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Unit>> DeleteCoupon(string code)
    {
        return HandleResult(await Mediator.Send(new DeleteCoupon.Command { Code = code }));
    }
}
