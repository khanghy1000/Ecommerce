using System.Security.Claims;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Security;

public class IsReviewOwnerRequirement : IAuthorizationRequirement { }

public class IsReviewOwnerRequirementHandler(
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

        if (userRole == nameof(UserRole.Admin))
        {
            context.Succeed(requirement);
            return;
        }

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.GetRouteValue("id") is not string reviewId)
            return;

        var review = await dbContext
            .ProductReviews.AsNoTracking()
            .SingleOrDefaultAsync(pr => pr.Id.ToString() == reviewId);

        if (review == null)
            return;

        if (review.UserId == userId)
            context.Succeed(requirement);
    }
}
