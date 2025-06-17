using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Reviews.Queries;
using Ecommerce.Tests.Application.Reviews.Helpers;
using FakeItEasy;
using Shouldly;

namespace Ecommerce.Tests.Application.Reviews.Queries;

public class GetReviewByIdTests
{
    private readonly IMapper _mapper = ReviewTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = ReviewTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task GetReviewById_ShouldReturnReview_WhenReviewExists()
    {
        // Arrange
        var reviewId = 1;
        var query = new GetReviewById.Query { ReviewId = reviewId };
        var handler = new GetReviewById.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(reviewId);
        result.Value.ProductId.ShouldBe(1);
        result.Value.UserId.ShouldBe("user1");
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnFailure_WhenReviewDoesNotExist()
    {
        // Arrange
        var reviewId = 999; // Non-existent review ID
        var query = new GetReviewById.Query { ReviewId = reviewId };
        var handler = new GetReviewById.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Review not found");
        result.Code.ShouldBe(404);
    }
}
