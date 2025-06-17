using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.SalesOrders.Commands;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.SalesOrders.Helpers;
using FakeItEasy;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.SalesOrders.Commands;

public class ConfirmOrderTests
{
    private readonly IMapper _mapper = SalesOrderTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = SalesOrderTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();
    private readonly IMediator _mediator = A.Fake<IMediator>();

    public ConfirmOrderTests()
    {
        // Set up fake mediator response for CreateShippingOrder command
        A.CallTo(() =>
                _mediator.Send(
                    A<CreateShippingOrder.Command>.That.Matches(c => c.SalesOrderId > 0),
                    A<CancellationToken>.Ignored
                )
            )
            .Returns(Result<Unit>.Success(Unit.Value));
    }

    [Fact]
    public async Task ConfirmOrder_ShouldChangeStatusToTracking_WhenOrderIsPendingConfirmation()
    {
        // Arrange
        // Find an order with PendingConfirmation status
        var pendingOrder = await _dbContext.SalesOrders.FirstOrDefaultAsync(so =>
            so.Status == SalesOrderStatus.PendingConfirmation
        );

        // If no PendingConfirmation order exists, create one
        if (pendingOrder == null)
        {
            var user = await _dbContext.Users.FirstAsync();
            var ward = await _dbContext.Wards.FirstAsync();

            pendingOrder = new SalesOrder
            {
                UserId = user.Id,
                ShippingName = "Test Name",
                ShippingPhone = "1234567890",
                ShippingAddress = "Test Address",
                ShippingWardId = ward.Id,
                ShippingFee = 10,
                Subtotal = 100,
                Total = 110,
                Status = SalesOrderStatus.PendingConfirmation,
                PaymentMethod = PaymentMethod.Cod,
            };

            _dbContext.SalesOrders.Add(pendingOrder);
            await _dbContext.SaveChangesAsync();
        }

        var command = new ConfirmOrder.Command { Id = pendingOrder.Id };
        var handler = new ConfirmOrder.Handler(_dbContext, _mapper, _mediator);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Status.ShouldBe(SalesOrderStatus.Tracking);

        // Verify the database was updated
        var updatedOrder = await _dbContext.SalesOrders.FindAsync(pendingOrder.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Status.ShouldBe(SalesOrderStatus.Tracking);
    }

    [Fact]
    public async Task ConfirmOrder_ShouldReturnError_WhenOrderIsNotInPendingConfirmationStatus()
    {
        // Arrange
        // Find an order that is not in PendingConfirmation status
        var nonPendingOrder = await _dbContext.SalesOrders.FirstOrDefaultAsync(so =>
            so.Status != SalesOrderStatus.PendingConfirmation
        );

        nonPendingOrder.ShouldNotBeNull();

        var command = new ConfirmOrder.Command { Id = nonPendingOrder.Id };
        var handler = new ConfirmOrder.Handler(_dbContext, _mapper, _mediator);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Sales order is not in pending confirmation status");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task ConfirmOrder_ShouldReturnError_WhenShippingOrderCreationFails()
    {
        // Arrange
        // Find a pending order
        var pendingOrder = await _dbContext.SalesOrders.FirstOrDefaultAsync(so =>
            so.Status == SalesOrderStatus.PendingConfirmation
        );

        // If no PendingConfirmation order exists, create one
        if (pendingOrder == null)
        {
            var user = await _dbContext.Users.FirstAsync();
            var ward = await _dbContext.Wards.FirstAsync();

            pendingOrder = new SalesOrder
            {
                UserId = user.Id,
                ShippingName = "Test Name",
                ShippingPhone = "1234567890",
                ShippingAddress = "Test Address",
                ShippingWardId = ward.Id,
                ShippingFee = 10,
                Subtotal = 100,
                Total = 110,
                Status = SalesOrderStatus.PendingConfirmation,
                PaymentMethod = PaymentMethod.Cod,
            };

            _dbContext.SalesOrders.Add(pendingOrder);
            await _dbContext.SaveChangesAsync();
        }

        // Override the mediator mock to return failure
        A.CallTo(() =>
                _mediator.Send(
                    A<CreateShippingOrder.Command>.That.Matches(c =>
                        c.SalesOrderId == pendingOrder.Id
                    ),
                    A<CancellationToken>.Ignored
                )
            )
            .Returns(Result<Unit>.Failure("Shipping order creation failed", 400));

        var command = new ConfirmOrder.Command { Id = pendingOrder.Id };
        var handler = new ConfirmOrder.Handler(_dbContext, _mapper, _mediator);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Shipping order creation failed");
        result.Code.ShouldBe(400);
    }
}
