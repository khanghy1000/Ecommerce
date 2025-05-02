using System.Security.Claims;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Security;

public class IsAddressOwnerRequirement : IAuthorizationRequirement { }

public class IsAddressOwnerRequirementHandler(
    AppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor
) : AuthorizationHandler<IsAddressOwnerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsAddressOwnerRequirement requirement
    )
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return;

        var userRole = context.User.FindFirstValue(ClaimTypes.Role);
        if (userRole == null)
            return;

        if (userRole == UserRole.Admin.ToString())
        {
            context.Succeed(requirement);
            return;
        }

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.GetRouteValue("addressId") is not int addressId)
            return;

        var address = await dbContext
            .UserAddresses.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == addressId);

        if (address == null)
            return;

        if (address.UserId == userId)
            context.Succeed(requirement);
    }
}
