using System.Security.Claims;
using Ecommerce.API.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Application.Users.Commands;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api")]
public class IdentityController(
    SignInManager<User> signInManager,
    RoleManager<IdentityRole> roleManager,
    IValidator<AddUserAddressRequestDto> addressValidator,
    AppDbContext dbContext
) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser(RegisterDto registerDto)
    {
        if (registerDto.Role == UserRole.Admin)
        {
            if (User.Identity?.IsAuthenticated == false)
                return Unauthorized();

            if (!User.IsInRole(UserRole.Admin.ToString()))
            {
                return Forbid();
            }
        }

        var validateAddressResult = await addressValidator.ValidateAsync(
            new AddUserAddressRequestDto
            {
                Name = registerDto.DisplayName,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                WardId = registerDto.WardId,
                IsDefault = true,
            }
        );

        if (!validateAddressResult.IsValid)
        {
            foreach (var error in validateAddressResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem();
        }

        var ward = await dbContext
            .Wards.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == registerDto.WardId);

        if (ward == null)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.6",
                    Title = "Ward not found",
                    Status = 400,
                    Instance = HttpContext.Request.Path,
                }
            );
        }

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            DisplayName = registerDto.DisplayName,
            PhoneNumber = registerDto.PhoneNumber,
        };

        var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            if (!await roleManager.RoleExistsAsync(registerDto.Role.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(registerDto.Role.ToString()));
            }

            var address = new UserAddress
            {
                Name = registerDto.DisplayName,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                WardId = registerDto.WardId,
                UserId = user.Id,
                IsDefault = true,
            };

            await signInManager.UserManager.AddToRoleAsync(user, registerDto.Role.ToString());
            await dbContext.UserAddresses.AddAsync(address);
            await dbContext.SaveChangesAsync();
            // await SendConfirmationEmailAsync(user, registerDto.Email);
            return Ok();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem();
    }

    [AllowAnonymous]
    [HttpGet("user-info")]
    public async Task<ActionResult> GetUserInfo()
    {
        if (User.Identity?.IsAuthenticated == false)
            return NoContent();

        var user = await signInManager.UserManager.Users.FirstOrDefaultAsync(u =>
            u.Email == User.FindFirstValue(ClaimTypes.Email)
        );

        if (user == null)
            return Unauthorized();

        var role = await signInManager.UserManager.GetRolesAsync(user);

        return Ok(
            new
            {
                user.DisplayName,
                user.Email,
                user.Id,
                user.ImageUrl,
                user.PhoneNumber,
                Role = role.FirstOrDefault(),
            }
        );
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto passwordDto)
    {
        var user = await signInManager.UserManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var result = await signInManager.UserManager.ChangePasswordAsync(
            user,
            passwordDto.CurrentPassword,
            passwordDto.NewPassword
        );

        if (result.Succeeded)
            return Ok();

        return BadRequest(result.Errors.First().Description);
    }
}
