using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Users.Commands;

public class SetDefaultUserAddress
{
    public class Command : IRequest<Result<UserAddressResponseDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor, IMapper mapper)
        : IRequestHandler<Command, Result<UserAddressResponseDto>>
    {
        public async Task<Result<UserAddressResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var userId = userAccessor.GetUserId();

            var address = await dbContext.UserAddresses.FirstOrDefaultAsync(
                a => a.UserId == userId && a.Id == request.Id,
                cancellationToken
            );

            if (address == null)
                return Result<UserAddressResponseDto>.Failure("Address not found", 404);

            if (!address.IsDefault)
            {
                var defaultAddress = await dbContext.UserAddresses.FirstOrDefaultAsync(
                    a => a.UserId == userId && a.IsDefault,
                    cancellationToken
                );

                if (defaultAddress != null)
                {
                    defaultAddress.IsDefault = false;
                    dbContext.UserAddresses.Update(defaultAddress);
                }

                address.IsDefault = true;
                dbContext.UserAddresses.Update(address);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return Result<UserAddressResponseDto>.Success(
                mapper.Map<UserAddressResponseDto>(address)
            );
        }
    }
}
