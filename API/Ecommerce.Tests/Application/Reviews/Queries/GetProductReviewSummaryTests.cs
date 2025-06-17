using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Reviews.Queries;
using Ecommerce.Tests.Application.Reviews.Helpers;
using Shouldly;

namespace Ecommerce.Tests.Application.Reviews.Queries;

public class GetProductReviewSummaryTests
{
    private readonly IMapper _mapper = ReviewTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = ReviewTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task GetProductReviewSummary_ShouldReturnSummary_WhenProductHasReviews()
    {
        // Arrange
        var productId = 1;
        var query = new GetProductReviewSummary.Query { ProductId = productId };
        var handler = new GetProductReviewSummary.Handler(_dbContext);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProductId.ShouldBe(productId);
        result.Value.AverageRating.ShouldBe(3); // Average of ratings 1,2,3,4,5 = 3
        result.Value.TotalReviews.ShouldBe(5);

        // Check distribution
        result.Value.RatingDistribution.ShouldContainKey(1);
        result.Value.RatingDistribution.ShouldContainKey(2);
        result.Value.RatingDistribution.ShouldContainKey(3);
        result.Value.RatingDistribution.ShouldContainKey(4);
        result.Value.RatingDistribution.ShouldContainKey(5);
        result.Value.RatingDistribution[1].ShouldBe(1);
        result.Value.RatingDistribution[2].ShouldBe(1);
        result.Value.RatingDistribution[3].ShouldBe(1);
        result.Value.RatingDistribution[4].ShouldBe(1);
        result.Value.RatingDistribution[5].ShouldBe(1);
    }
}
