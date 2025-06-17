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

public class AddToCartTests
{
    private readonly IMapper _mapper = CartTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly TestAppDbContext _dbContext = CartTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task AddToCart_ShouldAddItemToUserCart()
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
        var command = new AddToCart.Command
        {
            AddToCartRequestDto = new AddToCartRequestDto
            {
                ProductId = 2, // A product that's not yet in the cart
                Quantity = 3,
            },
        };
        var handler = new AddToCart.Handler(_dbContext, _userAccessor);
        var result = await handler.Handle(command, CancellationToken.None);

        // Check if item was added
        var cartItem = await _dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.UserId == user.Id && ci.ProductId == 2
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cartItem.ShouldNotBeNull();
        cartItem.Quantity.ShouldBe(3);
    }

    [Fact]
    public async Task AddToCart_ShouldUpdateQuantityIfItemAlreadyExists()
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
        var command = new AddToCart.Command
        {
            AddToCartRequestDto = new AddToCartRequestDto
            {
                ProductId = 1, // Already in the cart with quantity 2
                Quantity = 3,
            },
        };
        var handler = new AddToCart.Handler(_dbContext, _userAccessor);
        var result = await handler.Handle(command, CancellationToken.None);

        // Check if item quantity was updated
        var cartItem = await _dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.UserId == user.Id && ci.ProductId == 1
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cartItem.ShouldNotBeNull();
        cartItem.Quantity.ShouldBe(5); // 2 + 3 = 5
    }

    [Fact]
    public async Task AddToCart_ShouldLimitQuantityToAvailableStock()
    {
        // Setup
        var user = new User
        {
            Id = "test-user-id",
            DisplayName = "Test User",
            Email = "test@example.com",
        };
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(user);

        // Create a product with limited stock
        var product = await _dbContext.Products.FindAsync(3);
        product!.Quantity = 5; // Set available quantity to 5
        await _dbContext.SaveChangesAsync();

        // Act
        var command = new AddToCart.Command
        {
            AddToCartRequestDto = new AddToCartRequestDto
            {
                ProductId = 3,
                Quantity = 10, // Try to add more than available
            },
        };
        var handler = new AddToCart.Handler(_dbContext, _userAccessor);
        var result = await handler.Handle(command, CancellationToken.None);

        // Check if item was added with correct quantity
        var cartItem = await _dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.UserId == user.Id && ci.ProductId == 3
        );

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Code.ShouldBe(400);
    }
}
