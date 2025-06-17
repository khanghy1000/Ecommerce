using AutoMapper;
using Ecommerce.Application.CartItems.Commands;
using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.CartItems.Queries;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.CartItems.Helpers;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.CartItems.Queries;

public class GetCartItemsTests
{
    private readonly IMapper _mapper = CartTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly TestAppDbContext _dbContext = CartTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task GetCartItems_ShouldReturnUserCartItems()
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
        var query = new GetCartItems.Query();
        var handler = new GetCartItems.Handler(_dbContext, _userAccessor, _mapper);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<List<CartItemResponseDto>>();
        result.Value.Count.ShouldBeGreaterThan(0);
        // No need to check UserId as it's not in the DTO
    }
}
