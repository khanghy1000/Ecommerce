using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Users.Commands;

public class EditUserAddress
{
    public class Command : IRequest<Result<UserAddressResponseDto>>
    {
        public int Id { get; set; }
        public EditUserAddressRequestDto EditUserAddressRequestDto { get; set; } = null!;
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

            var ward = await dbContext.Wards.FirstOrDefaultAsync(
                w => w.Id == request.EditUserAddressRequestDto.WardId,
                cancellationToken
            );

            if (ward == null)
                return Result<UserAddressResponseDto>.Failure("Ward not found", 400);

            address.Name = request.EditUserAddressRequestDto.Name;
            address.PhoneNumber = request.EditUserAddressRequestDto.PhoneNumber;
            address.Address = request.EditUserAddressRequestDto.Address;
            address.WardId = request.EditUserAddressRequestDto.WardId;

            if (request.EditUserAddressRequestDto.IsDefault && !address.IsDefault)
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
            }

            if (!request.EditUserAddressRequestDto.IsDefault && address.IsDefault)
            {
                var addressCount = await dbContext.UserAddresses.CountAsync(
                    a => a.UserId == userId,
                    cancellationToken
                );

                if (addressCount <= 1)
                    return Result<UserAddressResponseDto>.Failure(
                        "Cannot remove the only default address",
                        400
                    );

                var newDefaultAddress = await dbContext.UserAddresses.FirstOrDefaultAsync(
                    a => a.UserId == userId && a.Id != address.Id,
                    cancellationToken
                );

                if (newDefaultAddress != null)
                {
                    address.IsDefault = false;
                    newDefaultAddress.IsDefault = true;
                    dbContext.UserAddresses.Update(newDefaultAddress);
                }
            }

            dbContext.UserAddresses.Update(address);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<UserAddressResponseDto>.Success(
                mapper.Map<UserAddressResponseDto>(address)
            );
        }
    }
}
