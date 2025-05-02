using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Users.Commands;

public class AddUserAddress
{
    public class Command : IRequest<Result<UserAddressResponseDto>>
    {
        public required AddUserAddressRequestDto AddUserAddressRequestDto { get; set; }
    }

    public class Handler(AppDbContext context, IUserAccessor userAccessor, IMapper mapper)
        : IRequestHandler<Command, Result<UserAddressResponseDto>>
    {
        public async Task<Result<UserAddressResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var user = await userAccessor.GetUserAsync();

            var ward = await context.Wards.FirstOrDefaultAsync(
                x => x.Id == request.AddUserAddressRequestDto.WardId,
                cancellationToken
            );

            if (ward == null)
                return Result<UserAddressResponseDto>.Failure("Ward not found", 400);

            var address = new UserAddress
            {
                Name = request.AddUserAddressRequestDto.Name,
                PhoneNumber = request.AddUserAddressRequestDto.PhoneNumber,
                Address = request.AddUserAddressRequestDto.Address,
                WardId = request.AddUserAddressRequestDto.WardId,
                UserId = user.Id,
                IsDefault = request.AddUserAddressRequestDto.IsDefault,
            };
            context.UserAddresses.Add(address);

            if (request.AddUserAddressRequestDto.IsDefault)
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

            return Result<UserAddressResponseDto>.Success(
                mapper.Map<UserAddressResponseDto>(address)
            );
        }
    }
}
