using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Application.Reviews.Queries;
using Ecommerce.Tests.Application.Reviews.Helpers;
using Shouldly;

namespace Ecommerce.Tests.Application.Reviews.Queries;

public class ListProductReviewsTests
{
    private readonly IMapper _mapper = ReviewTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = ReviewTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task ListProductReviews_ShouldReturnPagedList_WhenProductHasReviews()
    {
        // Arrange
        var query = new ListProductReviews.Query
        {
            ProductId = 1,
            PageNumber = 1,
            PageSize = 10,
        };

        var handler = new ListProductReviews.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<PagedList<ReviewResponseDto>>();
        result.Value.Items.Count.ShouldBe(5); // We created 5 reviews for product 1
        result.Value.TotalCount.ShouldBe(5);
        result.Value.PageNumber.ShouldBe(1);
        result.Value.PageSize.ShouldBe(10);
    }

    [Fact]
    public async Task ListProductReviews_ShouldReturnEmptyList_WhenProductHasNoReviews()
    {
        // Arrange
        var query = new ListProductReviews.Query
        {
            ProductId = 2, // Product 2 has no reviews
            PageNumber = 1,
            PageSize = 10,
        };

        var handler = new ListProductReviews.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Items.Count.ShouldBe(0);
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task ListProductReviews_ShouldReturnUserReviews_WhenUserIdIsProvided()
    {
        // Arrange
        var query = new ListProductReviews.Query
        {
            UserId = "user1",
            PageNumber = 1,
            PageSize = 10,
        };

        var handler = new ListProductReviews.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Items.Count.ShouldBe(5); // We created 5 reviews for user1
        result.Value.Items.ShouldAllBe(r => r.UserId == "user1");
    }
}
