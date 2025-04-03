using System.Security.Claims;
using Ecommerce.API.DTOs;
using Ecommerce.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api")]
public class IdentityController(
    SignInManager<User> signInManager,
    RoleManager<IdentityRole> roleManager
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

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            DisplayName = registerDto.DisplayName,
            PhoneNumber = registerDto.PhoneNumber,
            Address = registerDto.Address,
            WardId = registerDto.WardId,
        };

        var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            if (!await roleManager.RoleExistsAsync(registerDto.Role.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(registerDto.Role.ToString()));
            }

            await signInManager.UserManager.AddToRoleAsync(user, registerDto.Role.ToString());
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

        var user = await signInManager
            .UserManager.Users.Include(u => u.Ward)
            .ThenInclude(w => w.District)
            .ThenInclude(d => d.Province)
            .FirstOrDefaultAsync(u => u.Email == User.FindFirstValue(ClaimTypes.Email));

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
                user.Address,
                WardName = user.Ward?.Name,
                DistrictName = user.Ward?.District.Name,
                ProvinceName = user.Ward?.District.Province.Name,
                user.WardId,
                user.Ward?.DistrictId,
                user.Ward?.District.ProvinceId,
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
