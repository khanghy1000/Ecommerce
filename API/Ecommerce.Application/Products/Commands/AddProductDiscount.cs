using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class AddProductDiscount
{
    public class Command : IRequest<Result<ProductDiscountResponseDto>>
    {
        public required int ProductId { get; set; }
        public required AddProductDiscountRequestDto DiscountDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<ProductDiscountResponseDto>>
    {
        public async Task<Result<ProductDiscountResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.ProductId,
                cancellationToken
            );

            if (product == null)
                return Result<ProductDiscountResponseDto>.Failure("Product not found", 400);

            var hasOverlap = await dbContext.ProductDiscounts.AnyAsync(
                pd =>
                    pd.ProductId == request.ProductId
                    && (
                        // request start time is in the range of existing discount
                        (
                            pd.StartTime <= request.DiscountDto.StartTime
                            && pd.EndTime >= request.DiscountDto.StartTime
                        )
                        // request end time is in the range of existing discount
                        || (
                            pd.StartTime <= request.DiscountDto.EndTime
                            && pd.EndTime >= request.DiscountDto.EndTime
                        )
                        // existing discount is in the range of request
                        || (
                            pd.StartTime >= request.DiscountDto.StartTime
                            && pd.EndTime <= request.DiscountDto.EndTime
                        )
                    ),
                cancellationToken
            );

            if (hasOverlap)
                return Result<ProductDiscountResponseDto>.Failure(
                    "There is already a discount in this timeframe",
                    400
                );

            if (request.DiscountDto.DiscountPrice >= product.RegularPrice)
                return Result<ProductDiscountResponseDto>.Failure(
                    "Discount price must be less than regular price",
                    400
                );

            var discount = new ProductDiscount
            {
                ProductId = request.ProductId,
                DiscountPrice = request.DiscountDto.DiscountPrice,
                StartTime = request.DiscountDto.StartTime,
                EndTime = request.DiscountDto.EndTime,
            };

            dbContext.ProductDiscounts.Add(discount);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<ProductDiscountResponseDto>.Failure(
                    "Failed to add product discount",
                    400
                );

            return Result<ProductDiscountResponseDto>.Success(
                mapper.Map<ProductDiscountResponseDto>(discount)
            );
        }
    }
}
