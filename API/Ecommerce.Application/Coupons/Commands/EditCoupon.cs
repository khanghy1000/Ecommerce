using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Coupons.Commands;

public static class EditCoupon
{
    public class Command : IRequest<Result<CouponResponseDto>>
    {
        public string Code { get; set; } = string.Empty;
        public EditCouponRequestDto CouponRequest { get; set; } = new();
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<CouponResponseDto>>
    {
        public async Task<Result<CouponResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var coupon = await dbContext
                .Coupons.Include(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken);

            if (coupon == null)
            {
                return Result<CouponResponseDto>.Failure("Coupon not found", 404);
            }

            mapper.Map(request.CouponRequest, coupon);

            if (request.CouponRequest.CategoryIds != null)
            {
                coupon.Categories.Clear();

                if (request.CouponRequest.CategoryIds.Count > 0)
                {
                    var categories = await dbContext
                        .Categories.Where(c => request.CouponRequest.CategoryIds.Contains(c.Id))
                        .ToListAsync(cancellationToken);

                    foreach (var category in categories)
                    {
                        coupon.Categories.Add(category);
                    }
                }
            }

            dbContext.Coupons.Update(coupon);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<CouponResponseDto>.Success(mapper.Map<CouponResponseDto>(coupon));
        }
    }
}
