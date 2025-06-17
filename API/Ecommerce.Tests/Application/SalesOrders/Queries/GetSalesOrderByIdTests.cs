using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Application.SalesOrders.Queries;
using Ecommerce.Tests.Application.SalesOrders.Helpers;
using Shouldly;

namespace Ecommerce.Tests.Application.SalesOrders.Queries;

public class GetSalesOrderByIdTests
{
    private readonly IMapper _mapper = SalesOrderTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = SalesOrderTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task GetSalesOrderById_ShouldReturnSalesOrder_WhenIdExists()
    {
        // Arrange
        var query = new GetSalesOrderById.Query { Id = 1 };
        var handler = new GetSalesOrderById.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<SalesOrderResponseDto>();
        result.Value.Id.ShouldBe(1);
    }
}
