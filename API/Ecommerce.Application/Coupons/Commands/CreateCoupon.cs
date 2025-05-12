using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Coupons.Commands;

public static class CreateCoupon
{
    public class Command : IRequest<Result<CouponResponseDto>>
    {
        public CreateCouponRequestDto CouponRequest { get; set; } = new();
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<CouponResponseDto>>
    {
        public async Task<Result<CouponResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            if (
                await dbContext.Coupons.AnyAsync(
                    c => c.Code == request.CouponRequest.Code,
                    cancellationToken
                )
            )
            {
                return Result<CouponResponseDto>.Failure("Coupon code already exists", 400);
            }

            var coupon = mapper.Map<Coupon>(request.CouponRequest);

            if (request.CouponRequest.CategoryIds is { Count: > 0 })
            {
                var categories = await dbContext
                    .Categories.Where(c => request.CouponRequest.CategoryIds.Contains(c.Id))
                    .ToListAsync(cancellationToken);

                foreach (var category in categories)
                {
                    coupon.Categories.Add(category);
                }
            }

            dbContext.Coupons.Add(coupon);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<CouponResponseDto>.Success(mapper.Map<CouponResponseDto>(coupon));
        }
    }
}
