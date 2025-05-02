using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public class SetProductActiveState
{
    public class Command : IRequest<Result<Product>>
    {
        public required int ProductId { get; set; }
        public required bool IsActive { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<Product>>
    {
        public async Task<Result<Product>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.ProductId,
                cancellationToken: cancellationToken
            );

            if (product == null)
                return Result<Product>.Failure("Product not found", 400);

            product.Active = request.IsActive;

            dbContext.Products.Update(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Product>.Success(product);
        }
    }
}
