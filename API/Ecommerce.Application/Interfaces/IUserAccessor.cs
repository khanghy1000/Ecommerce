using Ecommerce.Domain;

namespace Ecommerce.Application.Interfaces;

public interface IUserAccessor
{
    string GetUserId();
    Task<User> GetUserAsync();
    IEnumerable<string> GetUserRoles();
}
