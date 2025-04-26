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
        if (userRole == null)
            return;

        if (userRole == UserRole.Admin.ToString())
            context.Succeed(requirement);

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.GetRouteValue("id") is not int orderId)
            return;

        var salesOrder = await dbContext
            .SalesOrders.Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (salesOrder == null)
            context.Succeed(requirement);

        if (userRole == UserRole.Buyer.ToString() && salesOrder!.UserId == userId)
            context.Succeed(requirement);

        if (
            userRole == UserRole.Shop.ToString()
            && salesOrder!.OrderProducts.Any(op => op.Product.ShopId == userId)
        )
            context.Succeed(requirement);
    }
}
