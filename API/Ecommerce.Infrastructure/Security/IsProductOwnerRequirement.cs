using System.Security.Claims;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Security;

public class IsProductOwnerRequirement : IAuthorizationRequirement { }

public class IsProductOwnerRequirementHandler(
    AppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor
) : AuthorizationHandler<IsProductOwnerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsProductOwnerRequirement requirement
    )
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return;

        var userRole = context.User.FindFirstValue(ClaimTypes.Role);
        if (userRole == null || userRole == UserRole.Buyer.ToString())
            return;

        if (userRole == UserRole.Admin.ToString())
            context.Succeed(requirement);

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.GetRouteValue("id") is not string productId)
            return;

        var product = await dbContext
            .Products.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id.ToString() == productId);

        if (product == null)
            return;

        if (product.ShopId == userId)
            context.Succeed(requirement);
    }
}
