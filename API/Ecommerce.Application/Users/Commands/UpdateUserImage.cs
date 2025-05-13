using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Application.Users.Commands;

public class UpdateUserImage
{
    public class Command : IRequest<Result<Unit>>
    {
        public required IFormFile File { get; set; }
    }

    public class Handler(
        AppDbContext dbContext,
        IPhotoService photoService,
        IUserAccessor userAccessor
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetUserAsync();

            var uploadResult = await photoService.UploadPhoto(
                request.File,
                $"photos/users/{user.Id}"
            );

            if (uploadResult == null)
                return Result<Unit>.Failure("Failed to upload photo", 400);

            user.ImageUrl = uploadResult.Key;
            dbContext.Users.Update(user);
            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return !result
                ? Result<Unit>.Failure("Failed to update user photo", 400)
                : Result<Unit>.Success(Unit.Value);
        }
    }
}
