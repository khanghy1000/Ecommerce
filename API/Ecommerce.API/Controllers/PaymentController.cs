using Ecommerce.Application.Core;
using Ecommerce.Application.Payments.Commands;
using Ecommerce.Application.Payments.DTOs;
using Ecommerce.Application.Payments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace Ecommerce.API.Controllers;

public class PaymentController() : BaseApiController
{
    // TODO: Remove this method
    [HttpPost("CreatePaymentUrl")]
    public async Task<ActionResult<string>> CreatePaymentUrl(
        CreatePaymentUrlRequestDto createPaymentUrlRequestDto
    )
    {
        var result = await Mediator.Send(
            new CreatePaymentUrl.Command { CreatePaymentUrlRequestDto = createPaymentUrlRequestDto }
        );
        return HandleResult(result);
    }

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
        return HandleResult(result);
    }
}
