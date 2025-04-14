using AutoMapper;
using Ecommerce.Application.CartItems.Commands;
using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.CartItems.Queries;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Ecommerce.Tests.Handlers;

public class CartItemHandlerTests
{
    private IMapper _mapper = GetMapper();
    private IUserAccessor _userAccessor = A.Fake<IUserAccessor>();

    private static IMapper GetMapper()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Application.Core.MappingProfiles>();
        });
        return mapperConfig.CreateMapper();
    }

    private static async Task<TestAppDbContext> GetDbContext()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestAppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        // Add test user
        var user = new User
        {
            Id = "test-user-id",
            UserName = "testuser",
            DisplayName = "Test User",
        };

        // Add test shop user
        var shop = new User
        {
            Id = "test-shop-id",
            UserName = "testshop",
            DisplayName = "Test Shop",
        };

        // Add test products
        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Test Product 1",
                Description = "Description for test product 1",
                RegularPrice = 100,
                DiscountPrice = 80,
                Quantity = 10,
                Active = true,
                ShopId = shop.Id,
                Length = 10,
                Width = 10,
                Height = 10,
                Weight = 1,
            },
            new Product
            {
                Id = 2,
                Name = "Test Product 2",
                Description = "Description for test product 2",
                RegularPrice = 200,
                Quantity = 5,
                Active = true,
                ShopId = shop.Id,
                Length = 20,
                Width = 20,
                Height = 20,
                Weight = 2,
            },
        };

        // Add test cart item
        var cartItems = new List<CartItem>
        {
            new CartItem
            {
                UserId = user.Id,
                ProductId = 1,
                Quantity = 2,
            },
        };

        await dbContext.Users.AddRangeAsync(user, shop);
        await dbContext.Products.AddRangeAsync(products);
        await dbContext.CartItems.AddRangeAsync(cartItems);
        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    [Fact]
    public async Task GetCartItems_ShouldReturnUserCartItems()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var handler = new GetCartItems.Handler(dbContext, _userAccessor, _mapper);
        var query = new GetCartItems.Query();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<List<CartItemResponseDto>>();
        result.Value.Count.ShouldBe(1);
        result.Value[0].ProductId.ShouldBe(1);
        result.Value[0].Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task GetCartItems_WithEmptyCart_ShouldReturnEmptyList()
    {
        // Arrange
        var dbContext = await GetDbContext();

        // Create a user with no cart items
        var emptyCartUser = new User
        {
            Id = "empty-cart-user",
            UserName = "emptyuser",
            DisplayName = "Empty Cart User",
        };
        dbContext.Users.Add(emptyCartUser);
        await dbContext.SaveChangesAsync();

        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(emptyCartUser));

        var handler = new GetCartItems.Handler(dbContext, _userAccessor, _mapper);
        var query = new GetCartItems.Query();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task AddToCart_WithNewProduct_ShouldAddCartItem()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new AddToCart.Command
        {
            AddToCartRequestDto = new AddToCartRequestDto
            {
                ProductId = 2, // Product that's not in the cart yet
                Quantity = 3,
            },
        };

        var handler = new AddToCart.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the item was added to the cart
        var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.ProductId == 2 && ci.UserId == user.Id
        );

        cartItem.ShouldNotBeNull();
        cartItem.Quantity.ShouldBe(3);
    }

    [Fact]
    public async Task AddToCart_WithExistingProduct_ShouldUpdateQuantity()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new AddToCart.Command
        {
            AddToCartRequestDto = new AddToCartRequestDto
            {
                ProductId = 1, // Product already in cart with quantity 2
                Quantity = 3,
            },
        };

        var handler = new AddToCart.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the item was updated in the cart
        var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.ProductId == 1 && ci.UserId == user.Id
        );

        cartItem.ShouldNotBeNull();
        cartItem.Quantity.ShouldBe(5); // 2 + 3 = 5
    }

    [Fact]
    public async Task AddToCart_WithInvalidProductId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new AddToCart.Command
        {
            AddToCartRequestDto = new AddToCartRequestDto
            {
                ProductId = 999, // Non-existent product
                Quantity = 1,
            },
        };

        var handler = new AddToCart.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Product not found");
    }

    [Fact]
    public async Task AddToCart_WithInsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new AddToCart.Command
        {
            AddToCartRequestDto = new AddToCartRequestDto
            {
                ProductId = 2, // Product with quantity 5
                Quantity = 10, // More than available stock
            },
        };

        var handler = new AddToCart.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Not enough product in stock");
    }

    [Fact]
    public async Task UpdateCartItem_ShouldUpdateQuantity()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new UpdateCartItem.Command
        {
            UpdateCartItemRequestDto = new UpdateCartItemRequestDto
            {
                ProductId = 1, // Existing product in cart
                Quantity = 4, // New quantity
            },
        };

        var handler = new UpdateCartItem.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the item was updated
        var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.ProductId == 1 && ci.UserId == user.Id
        );

        cartItem.ShouldNotBeNull();
        cartItem.Quantity.ShouldBe(4);
    }

    [Fact]
    public async Task UpdateCartItem_WithZeroQuantity_ShouldRemoveItem()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new UpdateCartItem.Command
        {
            UpdateCartItemRequestDto = new UpdateCartItemRequestDto
            {
                ProductId = 1, // Existing product in cart
                Quantity = 0, // Zero quantity should remove the item
            },
        };

        var handler = new UpdateCartItem.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the item was removed
        var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.ProductId == 1 && ci.UserId == user.Id
        );

        cartItem.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateCartItem_WithNonExistentItem_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new UpdateCartItem.Command
        {
            UpdateCartItemRequestDto = new UpdateCartItemRequestDto
            {
                ProductId = 999, // Non-existent product in cart
                Quantity = 5,
            },
        };

        var handler = new UpdateCartItem.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Cart item not found");
    }

    [Fact]
    public async Task UpdateCartItem_WithInsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new UpdateCartItem.Command
        {
            UpdateCartItemRequestDto = new UpdateCartItemRequestDto
            {
                ProductId = 1, // Product with quantity 10 in stock
                Quantity = 20, // More than available
            },
        };

        var handler = new UpdateCartItem.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Not enough product in stock");
    }

    [Fact]
    public async Task RemoveFromCart_ShouldRemoveCartItem()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new RemoveFromCart.Command
        {
            RemoveFromCartRequestDto = new RemoveFromCartRequestDto
            {
                ProductId = 1, // Existing product in cart
            },
        };

        var handler = new RemoveFromCart.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the item was removed
        var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.ProductId == 1 && ci.UserId == user.Id
        );

        cartItem.ShouldBeNull();
    }

    [Fact]
    public async Task RemoveFromCart_WithNonExistentItem_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();

        var user = await dbContext.Users.FirstAsync(u => u.Id == "test-user-id");
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(Task.FromResult(user));

        var command = new RemoveFromCart.Command
        {
            RemoveFromCartRequestDto = new RemoveFromCartRequestDto
            {
                ProductId = 999, // Non-existent product in cart
            },
        };

        var handler = new RemoveFromCart.Handler(dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Cart item not found");
    }
}
