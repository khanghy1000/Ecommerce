using AutoMapper;
using Ecommerce.Application.CartItems.Commands;
using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.CartItems.Queries;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.CartItems.Helpers;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.CartItems.Commands;

public class RemoveFromCartTests
{
    private readonly IMapper _mapper = CartTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly TestAppDbContext _dbContext = CartTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task RemoveFromCart_ShouldRemoveItemFromCart()
    {
        // Setup
        var user = new User
        {
            Id = "test-user-id",
            DisplayName = "Test User",
            Email = "test@example.com",
        };
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(user);

        // Act
        var command = new RemoveFromCart.Command
        {
            RemoveFromCartRequestDto = new RemoveFromCartRequestDto
            {
                ProductId = 1, // Existing cart item
            },
        };
        var handler = new RemoveFromCart.Handler(_dbContext, _userAccessor);
        var result = await handler.Handle(command, CancellationToken.None);

        // Check if item was removed
        var cartItem = await _dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.UserId == user.Id && ci.ProductId == 1
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cartItem.ShouldBeNull(); // Item should be removed
    }

    [Fact]
    public async Task RemoveFromCart_ShouldReturnErrorWhenItemNotFound()
    {
        // Setup
        var user = new User
        {
            Id = "test-user-id",
            DisplayName = "Test User",
            Email = "test@example.com",
        };
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(user);

        // Act
        var command = new RemoveFromCart.Command
        {
            RemoveFromCartRequestDto = new RemoveFromCartRequestDto
            {
                ProductId = 999, // Non-existent cart item
            },
        };
        var handler = new RemoveFromCart.Handler(_dbContext, _userAccessor);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Cart item not found");
        result.Code.ShouldBe(404);
    }
}
