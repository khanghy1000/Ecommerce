using Ecommerce.API.Controllers;
using Ecommerce.Application.Users.Commands;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Application.Users.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    [HttpGet("addresses")]
    public async Task<ActionResult<List<UserAddressResponseDto>>> GetUserAddresses()
    {
        return HandleResult(await Mediator.Send(new ListUserAddresses.Query()));
    }

    [HttpGet("addresses/{addressId}")]
    [Authorize(Policy = "IsAddressOwner")]
    public async Task<ActionResult<UserAddressResponseDto>> GetUserAddressById(int addressId)
    {
        return HandleResult(await Mediator.Send(new GetUserAddressById.Query { Id = addressId }));
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<UserAddressResponseDto>> AddAddress(
        AddUserAddressRequestDto userAddressDto
    )
    {
        return HandleResult(
            await Mediator.Send(
                new AddUserAddress.Command { AddUserAddressRequestDto = userAddressDto }
            )
        );
    }

    [HttpPut("addresses/default/{addressId}")]
    [Authorize(Policy = "IsAddressOwner")]
    public async Task<ActionResult<UserAddressResponseDto>> SetDefaultAddress(int addressId)
    {
        return HandleResult(
            await Mediator.Send(new SetDefaultUserAddress.Command { Id = addressId })
        );
    }

    [HttpPut("addresses/{addressId}")]
    [Authorize(Policy = "IsAddressOwner")]
    public async Task<ActionResult<UserAddressResponseDto>> EditAddress(
        int addressId,
        EditUserAddressRequestDto addressDto
    )
    {
        return HandleResult(
            await Mediator.Send(
                new EditUserAddress.Command
                {
                    Id = addressId,
                    EditUserAddressRequestDto = addressDto,
                }
            )
        );
    }

    [HttpDelete("addresses/{addressId}")]
    [Authorize(Policy = "IsAddressOwner")]
    public async Task<ActionResult<Unit>> DeleteAddress(int addressId)
    {
        return HandleResult(await Mediator.Send(new DeleteUserAddress.Command { Id = addressId }));
    }
}
