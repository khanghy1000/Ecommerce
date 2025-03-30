using Ecommerce.Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator =>
        _mediator ??=
            HttpContext.RequestServices.GetService<IMediator>()
            ?? throw new InvalidOperationException("IMediator service is unavailable");

    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        return result.IsSuccess switch
        {
            true when result.Value != null => Ok(result.Value),
            false when result.Code == 404 => NotFound(),
            _ => BadRequest(result.Error),
        };
    }
}
