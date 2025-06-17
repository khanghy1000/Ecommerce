using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Application.SalesOrders.Queries;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.SalesOrders.Helpers;
using FakeItEasy;
using Shouldly;

namespace Ecommerce.Tests.Application.SalesOrders.Queries;

public class ListSalesOrdersTests
{
    private readonly IMapper _mapper = SalesOrderTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = SalesOrderTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();
    private readonly IUserAccessor _userAccessor;
    private readonly User _testUser;

    public ListSalesOrdersTests()
    {
        _testUser = _dbContext.Users.First(u => u.DisplayName == "Test Buyer");
        _userAccessor = SalesOrderTestHelper.GetFakeUserAccessor(
            _testUser,
            UserRole.Buyer.ToString()
        );
    }

    [Fact]
    public async Task ListSalesOrders_ShouldReturnPagedList_WhenNoFiltersProvided()
    {
        // Arrange
        var query = new ListSalesOrders.Query();
        var handler = new ListSalesOrders.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<PagedList<SalesOrderResponseDto>>();
        result.Value.Items.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ListSalesOrders_ShouldReturnFilteredByStatus_WhenStatusFilterProvided()
    {
        // Arrange
        var statusFilter = SalesOrderStatus.PendingPayment;
        var query = new ListSalesOrders.Query { Status = statusFilter };
        var handler = new ListSalesOrders.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ShouldNotBeNull();
        result.Value.Items.ShouldAllBe(so => so.Status == statusFilter);
    }

    [Fact]
    public async Task ListSalesOrders_ShouldReturnFilteredByOrderId_WhenOrderIdFilterProvided()
    {
        // Arrange
        const int orderId = 1;
        var query = new ListSalesOrders.Query { OrderId = orderId };
        var handler = new ListSalesOrders.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ShouldNotBeNull();
        if (result.Value.Items.Any())
        {
            result.Value.Items.ShouldAllBe(so => so.Id == orderId);
        }
    }

    [Fact]
    public async Task ListSalesOrders_ShouldReturnPagedData_WhenPagingParametersProvided()
    {
        // Arrange
        var query = new ListSalesOrders.Query { PageNumber = 1, PageSize = 2 };
        var handler = new ListSalesOrders.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ShouldNotBeNull();
        result.Value.PageNumber.ShouldBe(1);
        result.Value.PageSize.ShouldBe(2);
        result.Value.Items.Count.ShouldBeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task ListSalesOrders_ShouldFilterByDateRange_WhenDateFiltersProvided()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-3);
        var toDate = DateTime.UtcNow;
        var query = new ListSalesOrders.Query { FromDate = fromDate, ToDate = toDate };
        var handler = new ListSalesOrders.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ShouldNotBeNull();
        result.Value.Items.ShouldAllBe(so => so.OrderTime >= fromDate && so.OrderTime <= toDate);
    }
}
