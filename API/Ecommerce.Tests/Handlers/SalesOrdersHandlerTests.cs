using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.Commands;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Ecommerce.Tests.Handlers;

public class SalesOrdersHandlerTests
{
    private readonly IMapper _mapper = GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IShippingService _shippingService = A.Fake<IShippingService>();
    private readonly IPaymentService _paymentService = A.Fake<IPaymentService>();

    private static IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());
        return config.CreateMapper();
    }

    private static async Task<TestAppDbContext> GetDbContext()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestAppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.SalesOrders.AnyAsync())
            return dbContext;

        // Create test user
        var user = new User
        {
            Id = "user-id",
            DisplayName = "Test User",
            Address = "Test Address",
            PhoneNumber = "0123456789",
            WardId = 1,
        };

        // Create test shop
        var shop = new User
        {
            Id = "shop-id",
            DisplayName = "Test Shop",
            Address = "Shop Address",
            PhoneNumber = "9876543210",
            WardId = 2,
        };

        dbContext.Users.AddRange(user, shop);

        // Create test products
        var products = new List<Product>();
        for (var i = 0; i < 5; i++)
        {
            products.Add(
                new Product
                {
                    Id = i + 1,
                    Name = $"Product {i}",
                    Description = $"Description {i}",
                    RegularPrice = 100 + i * 10,
                    DiscountPrice = 90 + i * 10,
                    Quantity = 20,
                    Active = true,
                    Length = 10,
                    Width = 10,
                    Height = 10,
                    Weight = 10,
                    ShopId = shop.Id,
                }
            );
        }
        dbContext.Products.AddRange(products);

        // Create test cart items
        var cartItems = new List<CartItem>
        {
            new CartItem
            {
                UserId = user.Id,
                ProductId = 1,
                Quantity = 2,
            },
            new CartItem
            {
                UserId = user.Id,
                ProductId = 2,
                Quantity = 1,
            },
        };
        dbContext.CartItems.AddRange(cartItems);

        // Create test locations
        var province = new Province { Id = 1, Name = "Test Province" };
        var district = new District
        {
            Id = 1,
            Name = "Test District",
            ProvinceId = 1,
        };
        var wards = new List<Ward>
        {
            new Ward
            {
                Id = 1,
                Name = "Ward 1",
                DistrictId = 1,
            },
            new Ward
            {
                Id = 2,
                Name = "Ward 2",
                DistrictId = 1,
            },
        };

        dbContext.Provinces.Add(province);
        dbContext.Districts.Add(district);
        dbContext.Wards.AddRange(wards);

        // Create test sales orders
        var salesOrders = new List<SalesOrder>
        {
            new SalesOrder
            {
                Id = 1,
                UserId = user.Id,
                Subtotal = 200,
                ShippingFee = 20,
                Total = 220,
                ShippingName = "Test User",
                ShippingPhone = "0123456789",
                ShippingAddress = "Test Address",
                ShippingWardId = 1,
                PaymentMethod = PaymentMethod.Cod,
                Status = SalesOrderStatus.PendingConfirmation,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
            },
            new SalesOrder
            {
                Id = 2,
                UserId = user.Id,
                Subtotal = 300,
                ShippingFee = 25,
                Total = 325,
                ShippingName = "Test User",
                ShippingPhone = "0123456789",
                ShippingAddress = "Test Address",
                ShippingWardId = 1,
                PaymentMethod = PaymentMethod.Vnpay,
                Status = SalesOrderStatus.PendingPayment,
                CreatedAt = DateTime.UtcNow.AddHours(-12),
            },
        };
        dbContext.SalesOrders.AddRange(salesOrders);

        // Create test order products
        var orderProducts = new List<OrderProduct>
        {
            new OrderProduct
            {
                OrderId = 1,
                ProductId = 1,
                Name = "Product 0",
                Price = 90,
                Quantity = 2,
                Subtotal = 180,
            },
            new OrderProduct
            {
                OrderId = 1,
                ProductId = 3,
                Name = "Product 2",
                Price = 110,
                Quantity = 1,
                Subtotal = 110,
            },
            new OrderProduct
            {
                OrderId = 2,
                ProductId = 2,
                Name = "Product 1",
                Price = 100,
                Quantity = 3,
                Subtotal = 300,
            },
        };
        dbContext.OrderProducts.AddRange(orderProducts);

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    [Fact]
    public async Task CheckoutPreview_ShouldReturnPreviewInformation()
    {
        // Arrange
        var dbContext = await GetDbContext();
        A.CallTo(() => _userAccessor.GetUserAsync())
            .Returns(dbContext.Users.First(u => u.Id == "user-id"));

        // Mock shipping service response
        A.CallTo(() => _shippingService.PreviewShipping(A<CreateShippingRequest>._))
            .Returns(
                new CreateShippingResponse
                {
                    Code = 200,
                    Data = new CreateShippingResponseData { TotalFee = 25000 },
                }
            );

        var command = new CheckoutPreview.Command
        {
            CheckoutPricePreviewRequestDto = new CheckoutPricePreviewRequestDto
            {
                ShippingWardId = 1,
            },
        };
        var handler = new CheckoutPreview.Handler(
            dbContext,
            _userAccessor,
            _mapper,
            _shippingService
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public async Task Checkout_WithCodPayment_ShouldCreateOrderAndReturnSuccess()
    {
        // Arrange
        var dbContext = await GetDbContext();
        A.CallTo(() => _userAccessor.GetUserAsync())
            .Returns(dbContext.Users.First(u => u.Id == "user-id"));

        // Mock shipping service response
        A.CallTo(() => _shippingService.PreviewShipping(A<CreateShippingRequest>._))
            .Returns(
                new CreateShippingResponse
                {
                    Code = 200,
                    Data = new CreateShippingResponseData { TotalFee = 25000 },
                }
            );

        var initialOrderCount = dbContext.SalesOrders.Count();
        var initialCartItemCount = dbContext.CartItems.Count();

        var command = new Checkout.Command
        {
            CheckoutRequestDto = new CheckoutRequestDto
            {
                ShippingName = "Test Checkout",
                ShippingPhone = "0987654321",
                ShippingAddress = "Test Address",
                ShippingWardId = 1,
                PaymentMethod = PaymentMethod.Cod,
            },
        };
        var handler = new Checkout.Handler(
            dbContext,
            _userAccessor,
            _mapper,
            _shippingService,
            _paymentService
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Check if order was created and cart items were removed
        dbContext.SalesOrders.Count().ShouldBeGreaterThan(initialOrderCount);
        dbContext.CartItems.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Checkout_WithVnpayPayment_ShouldCreateOrderWithPaymentUrl()
    {
        // Arrange
        var dbContext = await GetDbContext();
        A.CallTo(() => _userAccessor.GetUserAsync())
            .Returns(dbContext.Users.First(u => u.Id == "user-id"));

        // Mock shipping service response
        A.CallTo(() => _shippingService.PreviewShipping(A<CreateShippingRequest>._))
            .Returns(
                new CreateShippingResponse
                {
                    Code = 200,
                    Data = new CreateShippingResponseData { TotalFee = 25000 },
                }
            );

        A.CallTo(() => _paymentService.CreatePaymentUrl(A<double>._, A<string>._))
            .Returns("https://test-payment-url.com");

        var command = new Checkout.Command
        {
            CheckoutRequestDto = new CheckoutRequestDto
            {
                ShippingName = "Test Checkout",
                ShippingPhone = "0987654321",
                ShippingAddress = "Test Address",
                ShippingWardId = 1,
                PaymentMethod = PaymentMethod.Vnpay,
            },
        };
        var handler = new Checkout.Handler(
            dbContext,
            _userAccessor,
            _mapper,
            _shippingService,
            _paymentService
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PaymentUrl.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Checkout_WithEmptyCart_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        // Remove all cart items to simulate empty cart
        dbContext.CartItems.RemoveRange(dbContext.CartItems);
        await dbContext.SaveChangesAsync();

        A.CallTo(() => _userAccessor.GetUserAsync())
            .Returns(dbContext.Users.First(u => u.Id == "user-id"));

        var command = new Checkout.Command
        {
            CheckoutRequestDto = new CheckoutRequestDto
            {
                ShippingName = "Test Checkout",
                ShippingPhone = "0987654321",
                ShippingAddress = "Test Address",
                ShippingWardId = 1,
                PaymentMethod = PaymentMethod.Cod,
            },
        };
        var handler = new Checkout.Handler(
            dbContext,
            _userAccessor,
            _mapper,
            _shippingService,
            _paymentService
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Cart is empty");
    }
}
