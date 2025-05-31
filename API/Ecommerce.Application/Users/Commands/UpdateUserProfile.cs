using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Persistence;
using MediatR;

namespace Ecommerce.Application.Users.Commands;

public class UpdateUserProfile
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string DisplayName { get; set; }
    }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetUserAsync();
            user.DisplayName = request.DisplayName;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}