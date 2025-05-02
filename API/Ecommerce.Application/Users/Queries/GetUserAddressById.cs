using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Users.Queries;

public static class GetUserAddressById
{
    public class Query : IRequest<Result<UserAddressResponseDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<UserAddressResponseDto>>
    {
        public async Task<Result<UserAddressResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var userId = userAccessor.GetUserId();

            var address = await dbContext
                .UserAddresses.Include(a => a.Ward)
                .ThenInclude(w => w.District)
                .ThenInclude(d => d.Province)
                .Where(a => a.UserId == userId && a.Id == request.Id)
                .ProjectTo<UserAddressResponseDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (address == null)
                return Result<UserAddressResponseDto>.Failure("Address not found", 404);

            return Result<UserAddressResponseDto>.Success(address);
        }
    }
}
