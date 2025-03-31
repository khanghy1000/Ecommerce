using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class AddProductPhoto
{
    public class Command : IRequest<Result<ProductPhoto>>
    {
        public required int ProductId { get; set; }
        public required IFormFile File { get; set; }
    }

    public class Handler(AppDbContext dbContext, IPhotoService photoService)
        : IRequestHandler<Command, Result<ProductPhoto>>
    {
        public async Task<Result<ProductPhoto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var product = await dbContext.Products.FindAsync(request.ProductId);

            if (product == null)
                return Result<ProductPhoto>.Failure("Product not found", 404);

            var uploadResult = await photoService.UploadPhoto(
                request.File,
                $"photos/products/{request.ProductId}"
            );

            if (uploadResult == null)
                return Result<ProductPhoto>.Failure("Failed to upload photo", 400);

            var highestDisplayOrder = await dbContext
                .ProductPhotos.Where(p => p.ProductId == request.ProductId)
                .OrderByDescending(p => p.DisplayOrder)
                .Select(p => p.DisplayOrder)
                .FirstOrDefaultAsync(cancellationToken);

            var productPhoto = new ProductPhoto
            {
                Key = uploadResult.Key,
                ProductId = request.ProductId,
                DisplayOrder = highestDisplayOrder + 1,
            };

            dbContext.ProductPhotos.Add(productPhoto);

            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return !result
                ? Result<ProductPhoto>.Failure("Failed to add photo to product", 400)
                : Result<ProductPhoto>.Success(productPhoto);
        }
    }
}
