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
        return await dbContext
                .Users.Include(u => u.Ward)
                .ThenInclude(w => w!.District)
                .ThenInclude(d => d.Province)
                .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("No user is logged in");
    }

    public string GetUserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("No user found");
    }
}
