using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Users.Commands;

public class AddAddress
{
    public class Command : IRequest<Result<Unit>>
    {
        public required AddAddressRequestDto AddAddressRequestDto { get; set; }
    }

    public class Handler(AppDbContext context, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetUserAsync();

            var ward = await context.Wards.FirstOrDefaultAsync(
                x => x.Id == request.AddAddressRequestDto.WardId,
                cancellationToken
            );

            if (ward == null)
                return Result<Unit>.Failure("Ward not found", 400);

            var address = new UserAddress
            {
                Name = request.AddAddressRequestDto.Name,
                PhoneNumber = request.AddAddressRequestDto.PhoneNumber,
                Address = request.AddAddressRequestDto.Address,
                WardId = request.AddAddressRequestDto.WardId,
                UserId = user.Id,
                IsDefault = request.AddAddressRequestDto.IsDefault,
            };
            context.UserAddresses.Add(address);

            if (request.AddAddressRequestDto.IsDefault)
            {
                var defaultAddress = await context.UserAddresses.FirstOrDefaultAsync(
                    x => x.UserId == user.Id && x.IsDefault,
                    cancellationToken
                );

                if (defaultAddress != null)
                {
                    defaultAddress.IsDefault = false;
                    context.UserAddresses.Update(defaultAddress);
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
