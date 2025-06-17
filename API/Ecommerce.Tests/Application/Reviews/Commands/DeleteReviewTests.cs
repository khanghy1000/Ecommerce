using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Reviews.Commands;
using Ecommerce.Tests.Application.Reviews.Helpers;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Reviews.Commands;

public class DeleteReviewTests
{
    private readonly IMapper _mapper = ReviewTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = ReviewTestHelper.GetUserAccessor();
    private readonly TestAppDbContext _dbContext = ReviewTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task DeleteReview_ShouldDeleteExistingReview_WhenValidRequest()
    {
        // Arrange
        var reviewId = 1;
        var command = new DeleteReview.Command { ReviewId = reviewId };
        var handler = new DeleteReview.Handler(_dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the review was deleted from the database
        var deletedReview = await _dbContext.ProductReviews.FindAsync(reviewId);
        deletedReview.ShouldBeNull();
    }
}
