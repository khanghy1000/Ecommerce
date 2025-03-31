using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class DeleteProductPhoto
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string Key { get; set; }
        public required int ProductId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IPhotoService photoService)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var photo = await dbContext.ProductPhotos.FirstOrDefaultAsync(
                p => p.Key == request.Key && p.ProductId == request.ProductId,
                cancellationToken
            );

            if (photo == null)
                return Result<Unit>.Failure("Photo not found", 404);

            await photoService.DeletePhoto(photo.Key);
            dbContext.Remove(photo);

            var remainingPhotos = await dbContext
                .ProductPhotos.Where(p => p.ProductId == request.ProductId && p.Key != request.Key)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);

            for (var i = 0; i < remainingPhotos.Count; i++)
            {
                remainingPhotos[i].DisplayOrder = i + 1;
            }

            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return !result
                ? Result<Unit>.Failure("Failed to delete photo", 400)
                : Result<Unit>.Success(Unit.Value);
        }
    }
}
