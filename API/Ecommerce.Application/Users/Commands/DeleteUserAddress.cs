using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Users.Commands;

public class DeleteUserAddress
{
    public class Command : IRequest<Result<Unit>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var address = await dbContext.UserAddresses.FirstOrDefaultAsync(
                a => a.UserId == userId && a.Id == request.Id,
                cancellationToken
            );

            if (address == null)
                return Result<Unit>.Failure("Address not found", 404);

            if (address.IsDefault)
            {
                return Result<Unit>.Failure("Cannot delete default address", 400);
            }

            dbContext.UserAddresses.Remove(address);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
