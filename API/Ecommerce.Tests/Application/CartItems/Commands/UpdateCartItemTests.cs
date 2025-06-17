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

public class UpdateCartItemTests
{
    private readonly IMapper _mapper = CartTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly TestAppDbContext _dbContext = CartTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task UpdateCartItem_ShouldUpdateItemQuantity()
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
        var command = new UpdateCartItem.Command
        {
            UpdateCartItemRequestDto = new UpdateCartItemRequestDto
            {
                ProductId = 1, // Already in the cart with quantity 2
                Quantity = 4,
            },
        };
        var handler = new UpdateCartItem.Handler(_dbContext, _userAccessor);
        var result = await handler.Handle(command, CancellationToken.None);

        // Check if item quantity was updated
        var cartItem = await _dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.UserId == user.Id && ci.ProductId == 1
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cartItem.ShouldNotBeNull();
        cartItem.Quantity.ShouldBe(4);
    }

    [Fact]
    public async Task UpdateCartItem_ShouldRemoveItemWhenQuantityIsZeroOrLess()
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
        var command = new UpdateCartItem.Command
        {
            UpdateCartItemRequestDto = new UpdateCartItemRequestDto
            {
                ProductId = 1, // Existing cart item
                Quantity = 0,
            },
        };
        var handler = new UpdateCartItem.Handler(_dbContext, _userAccessor);
        var result = await handler.Handle(command, CancellationToken.None);

        // Check if item was removed
        var cartItem = await _dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.UserId == user.Id && ci.ProductId == 1
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cartItem.ShouldBeNull(); // Item should be removed
    }
}
