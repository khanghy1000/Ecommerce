using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Users.Queries;

public static class ListUserAddresses
{
    public class Query : IRequest<Result<List<UserAddressResponseDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<List<UserAddressResponseDto>>>
    {
        public async Task<Result<List<UserAddressResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var userId = userAccessor.GetUserId();

            var addresses = await dbContext
                .UserAddresses.Include(a => a.Ward)
                .ThenInclude(w => w.District)
                .ThenInclude(d => d.Province)
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Name)
                .ProjectTo<UserAddressResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<UserAddressResponseDto>>.Success(addresses);
        }
    }
}
