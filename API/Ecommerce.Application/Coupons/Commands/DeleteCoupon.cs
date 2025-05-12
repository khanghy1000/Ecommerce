using Ecommerce.Application.Core;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Coupons.Commands;

public static class DeleteCoupon
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Code { get; set; } = string.Empty;
    }

    public class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var coupon = await dbContext
                .Coupons.Include(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken);

            if (coupon == null)
            {
                return Result<Unit>.Failure("Coupon not found", 404);
            }

            dbContext.Coupons.Remove(coupon);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
