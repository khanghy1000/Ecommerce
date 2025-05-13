using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class EditProductDiscount
{
    public class Command : IRequest<Result<ProductDiscountResponseDto>>
    {
        public required int ProductId { get; set; }
        public required int DiscountId { get; set; }
        public required EditProductDiscountRequestDto DiscountDto { get; set; }
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

            var discount = await dbContext.ProductDiscounts.FirstOrDefaultAsync(
                d => d.Id == request.DiscountId && d.ProductId == request.ProductId,
                cancellationToken
            );

            if (discount == null)
                return Result<ProductDiscountResponseDto>.Failure("Discount not found", 400);

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

            // Check if discount price is less than regular price
            if (request.DiscountDto.DiscountPrice >= product.RegularPrice)
                return Result<ProductDiscountResponseDto>.Failure(
                    "Discount price must be less than regular price",
                    400
                );

            discount.DiscountPrice = request.DiscountDto.DiscountPrice;
            discount.StartTime = request.DiscountDto.StartTime;
            discount.EndTime = request.DiscountDto.EndTime;

            dbContext.ProductDiscounts.Update(discount);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<ProductDiscountResponseDto>.Success(
                mapper.Map<ProductDiscountResponseDto>(discount)
            );
        }
    }
}
