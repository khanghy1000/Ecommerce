using System.Security.Claims;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Security;

public class HasOrderRequirement : IAuthorizationRequirement { }

public class HasOrderRequirementHandler(
    AppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor
) : AuthorizationHandler<HasOrderRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasOrderRequirement requirement
    )
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return;

        var userRole = context.User.FindFirstValue(ClaimTypes.Role);
        if (userRole == null)
            return;

        if (userRole == nameof(UserRole.Admin))
            context.Succeed(requirement);

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.GetRouteValue("id") is not string orderId)
            return;

        var salesOrder = await dbContext
            .SalesOrders.Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

        if (salesOrder == null)
            return;

        if (userRole == nameof(UserRole.Buyer) && salesOrder!.UserId == userId)
            context.Succeed(requirement);

        if (
            userRole == nameof(UserRole.Shop)
            && salesOrder!.OrderProducts.Any(op => op.Product.ShopId == userId)
        )
            context.Succeed(requirement);
    }
}
