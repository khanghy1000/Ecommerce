using Ecommerce.Application.Payments.Commands;
using Ecommerce.Application.Payments.DTOs;
using Ecommerce.Application.Payments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNPAY.NET.Models;

namespace Ecommerce.API.Controllers;

[AllowAnonymous]
public class PaymentController(IConfiguration config) : BaseApiController
{
    /// Perform any necessary actions after payment. This URL needs to be declared with VNPAY to work (e.g., http://localhost:1234/api/Vnpay/IpnAction)
    [HttpGet("IpnAction")]
    public async Task<ActionResult<Unit>> IpnAction()
    {
        // TODO: Replace with actual logic
        var result = await Mediator.Send(new GetPaymentResult.Query());
        return Ok();
    }

    /// Return the result of the payment.
    [HttpGet("Callback")]
    public async Task<ActionResult<PaymentResult>> Callback()
    {
        var result = await Mediator.Send(new PaymentCallback.Query());
        return result.IsSuccess
            ? Redirect(
                config["ClientUrl"] + $"/payment/success?paymentId={result.Value!.PaymentId}"
            )
            : Redirect(
                config["ClientUrl"] + $"/payment/failure?paymentId={result.Value!.PaymentId}"
            );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentResponseDto>> GetPaymentById(long id)
    {
        var result = await Mediator.Send(new GetPaymentById.Query { PaymentId = id });
        return HandleResult(result);
    }
}
