using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.Commands;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.SalesOrders.Helpers;
using FakeItEasy;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.SalesOrders.Commands;

public class CheckoutTests
{
    private readonly IMapper _mapper = SalesOrderTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = SalesOrderTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();
    private readonly IUserAccessor _userAccessor;
    private readonly IShippingService _shippingService =
        SalesOrderTestHelper.GetFakeShippingService();
    private readonly IPaymentService _paymentService = SalesOrderTestHelper.GetFakePaymentService();
    private readonly IMediator _mediator = A.Fake<IMediator>();
    private readonly User _testUser;

    public CheckoutTests()
    {
        _testUser = _dbContext.Users.First(u => u.DisplayName == "Test Buyer");
        _userAccessor = SalesOrderTestHelper.GetFakeUserAccessor(
            _testUser,
            UserRole.Buyer.ToString()
        );

        // Set up CartItems for testing
        SetupCartItems().GetAwaiter().GetResult();

        // Set up fake shipping service response
        A.CallTo(() => _shippingService.PreviewShipping(A<CreateShippingRequest>.Ignored))
            .Returns(
                Task.FromResult<CreateShippingResponse?>(
                    new CreateShippingResponse
                    {
                        Code = 200,
                        Message = "Success",
                        Data = new CreateShippingResponseData
                        {
                            OrderCode =
                                $"FAKE{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                            SortCode = "A1.B2.C3",
                            TransType = "truck",
                            WardEncode = "W02",
                            DistrictEncode = "D02",
                            TotalFee = 20000,
                            ExpectedDeliveryTime = DateTime.Now.AddDays(3),
                            Fee = new CreateShippingResponseDataFee
                            {
                                MainService = 15000,
                                Insurance = 5000,
                                StationDo = 0,
                                StationPu = 0,
                                Return = 0,
                                R2S = 0,
                                Coupon = 0,
                                CodFailedFee = 0,
                            },
                        },
                    }
                )
            );

        // Set up fake shipping service creation response
        A.CallTo(() => _shippingService.CreateShipping(A<CreateShippingRequest>.Ignored))
            .Returns(
                Task.FromResult<CreateShippingResponse?>(
                    new CreateShippingResponse
                    {
                        Code = 200,
                        Message = "Success",
                        Data = new CreateShippingResponseData
                        {
                            OrderCode =
                                $"ORDER{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                            SortCode = "A1.B2.C3",
                            TransType = "truck",
                            WardEncode = "W02",
                            DistrictEncode = "D02",
                            TotalFee = 20000,
                            ExpectedDeliveryTime = DateTime.Now.AddDays(3),
                            Fee = new CreateShippingResponseDataFee
                            {
                                MainService = 15000,
                                Insurance = 5000,
                                StationDo = 0,
                                StationPu = 0,
                                Return = 0,
                                R2S = 0,
                                Coupon = 0,
                                CodFailedFee = 0,
                            },
                        },
                    }
                )
            );
    }

    private async Task SetupCartItems()
    {
        // Clear any existing cart items
        var existingItems = await _dbContext.CartItems.ToListAsync();
        _dbContext.CartItems.RemoveRange(existingItems);

        // Add cart items for testing
        var products = await _dbContext.Products.Take(3).ToListAsync();

        foreach (var product in products)
        {
            _dbContext.CartItems.Add(
                new CartItem
                {
                    UserId = _testUser.Id,
                    ProductId = product.Id,
                    Quantity = 2,
                    Product = product,
                }
            );
        }

        await _dbContext.SaveChangesAsync();
    }

    private CheckoutRequestDto CreateValidCheckoutRequest()
    {
        return new CheckoutRequestDto
        {
            ProductIds = _dbContext.Products.Take(2).Select(p => p.Id).ToList(),
            ShippingName = "Test Recipient",
            ShippingPhone = "1234567890",
            ShippingAddress = "Test Address",
            ShippingWardId = 1,
            PaymentMethod = PaymentMethod.Cod,
        };
    }

    [Fact]
    public async Task Checkout_ShouldCreateOrder_WhenRequestIsValid()
    {
        // Arrange
        var checkoutRequest = CreateValidCheckoutRequest();
        var command = new Checkout.Command { CheckoutRequestDto = checkoutRequest };
        var handler = new Checkout.Handler(
            _dbContext,
            _userAccessor,
            _mapper,
            _shippingService,
            _paymentService,
            _mediator
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.SalesOrders.ShouldNotBeEmpty();
        result.Value.SalesOrders[0].Id.ShouldBeGreaterThan(0);

        // Verify order was created in database
        var createdOrder = await _dbContext
            .SalesOrders.Include(o => o.OrderProducts)
            .FirstOrDefaultAsync(o => o.Id == result.Value.SalesOrders[0].Id);

        createdOrder.ShouldNotBeNull();
        createdOrder.UserId.ShouldBe(_testUser.Id);
        createdOrder.ShippingName.ShouldBe(checkoutRequest.ShippingName);
        createdOrder.OrderProducts.Count.ShouldBe(checkoutRequest.ProductIds.Count);
    }

    [Fact]
    public async Task Checkout_ShouldReturnError_WhenWardNotFound()
    {
        // Arrange
        var checkoutRequest = CreateValidCheckoutRequest();
        checkoutRequest.ShippingWardId = 999; // Non-existent ward ID
        var command = new Checkout.Command { CheckoutRequestDto = checkoutRequest };
        var handler = new Checkout.Handler(
            _dbContext,
            _userAccessor,
            _mapper,
            _shippingService,
            _paymentService,
            _mediator
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Ward not found");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task Checkout_ShouldReturnError_WhenCartIsEmpty()
    {
        // Arrange
        var checkoutRequest = CreateValidCheckoutRequest();
        checkoutRequest.ProductIds = new List<int> { 999 }; // Non-existent product ID
        var command = new Checkout.Command { CheckoutRequestDto = checkoutRequest };
        var handler = new Checkout.Handler(
            _dbContext,
            _userAccessor,
            _mapper,
            _shippingService,
            _paymentService,
            _mediator
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Cart is empty");
        result.Code.ShouldBe(400);
    }
}
