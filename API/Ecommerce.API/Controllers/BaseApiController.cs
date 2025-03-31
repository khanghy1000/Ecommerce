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
            false when result.Code == 404 => NotFound(
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Title = result.Error ?? "Not Found",
                    Status = 404,
                    Instance = HttpContext.Request.Path,
                }
            ),
            _ => BadRequest(
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.6",
                    Title = result.Error ?? "Bad Request",
                    Status = 400,
                    Instance = HttpContext.Request.Path,
                }
            ),
        };
    }
}
