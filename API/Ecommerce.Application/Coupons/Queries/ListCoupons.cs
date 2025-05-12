using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Coupons.Queries;

public static class ListCoupons
{
    public class Query : IRequest<Result<List<CouponResponseDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<CouponResponseDto>>>
    {
        public async Task<Result<List<CouponResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var coupons = await dbContext
                .Coupons.ProjectTo<CouponResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<CouponResponseDto>>.Success(coupons);
        }
    }
}
