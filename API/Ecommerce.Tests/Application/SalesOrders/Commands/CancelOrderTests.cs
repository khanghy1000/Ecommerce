using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.SalesOrders.Commands;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.SalesOrders.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.SalesOrders.Commands;

public class CancelOrderTests
{
    private readonly IMapper _mapper = SalesOrderTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = SalesOrderTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task CancelOrder_ShouldChangeStatusToCancelled_WhenOrderIsInPendingConfirmation()
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

        var command = new CancelOrder.Command { Id = pendingOrder.Id };
        var handler = new CancelOrder.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Status.ShouldBe(SalesOrderStatus.Cancelled);

        // Verify the database was updated
        var updatedOrder = await _dbContext.SalesOrders.FindAsync(pendingOrder.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Status.ShouldBe(SalesOrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrder_ShouldChangeStatusToCancelled_WhenOrderIsInPendingPayment()
    {
        // Arrange
        // Find an order with PendingPayment status
        var pendingOrder = await _dbContext.SalesOrders.FirstOrDefaultAsync(so =>
            so.Status == SalesOrderStatus.PendingPayment
        );

        // If no PendingPayment order exists, create one
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
                Status = SalesOrderStatus.PendingPayment,
                PaymentMethod = PaymentMethod.Vnpay,
            };

            _dbContext.SalesOrders.Add(pendingOrder);
            await _dbContext.SaveChangesAsync();
        }

        var command = new CancelOrder.Command { Id = pendingOrder.Id };
        var handler = new CancelOrder.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Status.ShouldBe(SalesOrderStatus.Cancelled);

        // Verify the database was updated
        var updatedOrder = await _dbContext.SalesOrders.FindAsync(pendingOrder.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Status.ShouldBe(SalesOrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrder_ShouldReturnError_WhenOrderIsInInvalidStatus()
    {
        // Arrange
        // Find an order that is in delivered or tracking status
        var invalidStatusOrder = await _dbContext.SalesOrders.FirstOrDefaultAsync(so =>
            so.Status == SalesOrderStatus.Delivered || so.Status == SalesOrderStatus.Tracking
        );

        if (invalidStatusOrder == null)
        {
            // Create one if it doesn't exist
            var user = await _dbContext.Users.FirstAsync();
            var ward = await _dbContext.Wards.FirstAsync();

            invalidStatusOrder = new SalesOrder
            {
                UserId = user.Id,
                ShippingName = "Test Name",
                ShippingPhone = "1234567890",
                ShippingAddress = "Test Address",
                ShippingWardId = ward.Id,
                ShippingFee = 10,
                Subtotal = 100,
                Total = 110,
                Status = SalesOrderStatus.Tracking,
                PaymentMethod = PaymentMethod.Cod,
            };

            _dbContext.SalesOrders.Add(invalidStatusOrder);
            await _dbContext.SaveChangesAsync();
        }

        var command = new CancelOrder.Command { Id = invalidStatusOrder.Id };
        var handler = new CancelOrder.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(
            "Sales order is not in pending confirmation or pending payment status"
        );
        result.Code.ShouldBe(400);
    }
}
