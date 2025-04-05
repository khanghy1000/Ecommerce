using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Products.Commands;

public static class UpdateProductPhotoDisplayOrder
{
    public class Command : IRequest<Result<Unit>>
    {
        public required int ProductId { get; set; }
        public required List<UpdateProductPhotoDisplayOrderRequestDto> PhotoOrders { get; set; }
    }

    public class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.ProductId,
                cancellationToken
            );

            if (product == null)
                return Result<Unit>.Failure("Product not found", 404);

            var productPhotos = await dbContext
                .ProductPhotos.Where(p => p.ProductId == request.ProductId)
                .ToListAsync(cancellationToken);

            // Create a dictionary for fast lookups
            var photoDict = productPhotos.ToDictionary(p => p.Key);

            foreach (
                var orderItem in request.PhotoOrders.Where(orderItem =>
                    !photoDict.ContainsKey(orderItem.Key)
                )
            )
            {
                return Result<Unit>.Failure($"Photo with key '{orderItem.Key}' not found", 404);
            }

            foreach (var orderItem in request.PhotoOrders)
            {
                photoDict[orderItem.Key].DisplayOrder = orderItem.DisplayOrder;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
