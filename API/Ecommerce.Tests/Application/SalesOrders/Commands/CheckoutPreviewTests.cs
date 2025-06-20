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

public class CheckoutPreviewTests
{
    private readonly IMapper _mapper = SalesOrderTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = SalesOrderTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();
    private readonly IUserAccessor _userAccessor;
    private readonly IShippingService _shippingService =
        SalesOrderTestHelper.GetFakeShippingService();
    private readonly IMediator _mediator = A.Fake<IMediator>();
    private readonly User _testUser;

    public CheckoutPreviewTests()
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

    private CheckoutPricePreviewRequestDto CreateValidPreviewRequest()
    {
        return new CheckoutPricePreviewRequestDto
        {
            ProductIds = _dbContext.Products.Take(2).Select(p => p.Id).ToList(),
            ShippingWardId = 1,
            ProductCouponCode = null,
            ShippingCouponCode = null,
        };
    }

    [Fact]
    public async Task CheckoutPreview_ShouldCalculateCorrectAmount_WhenRequestIsValid()
    {
        // Arrange
        var previewRequest = CreateValidPreviewRequest();
        var command = new CheckoutPreview.Command
        {
            CheckoutPricePreviewRequestDto = previewRequest,
        };
        var handler = new CheckoutPreview.Handler(
            _dbContext,
            _userAccessor,
            _mapper,
            _shippingService,
            _mediator
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Subtotal.ShouldBeGreaterThan(0);
        result.Value.ShippingFee.ShouldBeGreaterThan(0);
        result.Value.Total.ShouldBe(
            result.Value.Subtotal
                + result.Value.ShippingFee
                - result.Value.ProductDiscountAmount
                - result.Value.ShippingDiscountAmount
        );
    }

    [Fact]
    public async Task CheckoutPreview_ShouldReturnError_WhenWardNotFound()
    {
        // Arrange
        var previewRequest = CreateValidPreviewRequest();
        previewRequest.ShippingWardId = 999; // Non-existent ward
        var command = new CheckoutPreview.Command
        {
            CheckoutPricePreviewRequestDto = previewRequest,
        };
        var handler = new CheckoutPreview.Handler(
            _dbContext,
            _userAccessor,
            _mapper,
            _shippingService,
            _mediator
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Ward not found");
        result.Code.ShouldBe(400);
    }
}
