using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Reviews.Commands;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Tests.Application.Reviews.Helpers;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Reviews.Commands;

public class UpdateReviewTests
{
    private readonly IMapper _mapper = ReviewTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = ReviewTestHelper.GetUserAccessor();
    private readonly TestAppDbContext _dbContext = ReviewTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task UpdateReview_ShouldUpdateExistingReview_WhenValidRequest()
    {
        // Arrange
        var reviewId = 1;
        var reviewDto = new UpdateReviewRequestDto
        {
            Rating = 5,
            Review = "Updated review content",
        };

        var command = new UpdateReview.Command { ReviewId = reviewId, ReviewDto = reviewDto };
        var handler = new UpdateReview.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(reviewId);
        result.Value.Rating.ShouldBe(5);
        result.Value.Review.ShouldBe("Updated review content");

        // Verify the review was updated in the database
        var updatedReview = await _dbContext.ProductReviews.FindAsync(reviewId);
        updatedReview.ShouldNotBeNull();
        updatedReview.Rating.ShouldBe(5);
        updatedReview.Review.ShouldBe("Updated review content");
    }
}
