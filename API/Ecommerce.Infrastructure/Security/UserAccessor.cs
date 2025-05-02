using System.Security.Claims;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Security;

public class UserAccessor(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
    : IUserAccessor
{
    public async Task<User> GetUserAsync()
    {
        var userId = GetUserId();
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("No user is logged in");
    }

    public string GetUserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("No user found");
    }

    public IEnumerable<string> GetUserRoles()
    {
        var claims = httpContextAccessor.HttpContext?.User.Claims;
        return claims?.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
            ?? new List<string>();
    }
}
