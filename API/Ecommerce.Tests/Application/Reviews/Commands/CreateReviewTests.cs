using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Reviews.Commands;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Reviews.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Reviews.Commands;

public class CreateReviewTests
{
    private readonly IMapper _mapper = ReviewTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = ReviewTestHelper.GetUserAccessor();
    private readonly TestAppDbContext _dbContext = ReviewTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task CreateReview_ShouldCreateNewReview_WhenValidRequest()
    {
        // Arrange
        var reviewDto = new CreateReviewRequestDto
        {
            ProductId = 2, // Using product 2 which has not been reviewed yet but has been purchased
            Rating = 4,
            Review = "This is a test review",
        };

        var command = new CreateReview.Command { ReviewDto = reviewDto };
        var handler = new CreateReview.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Rating.ShouldBe(4);
        result.Value.Review.ShouldBe("This is a test review");
        result.Value.ProductId.ShouldBe(2);
        result.Value.UserId.ShouldBe("user1");

        // Verify the review was added to database
        var savedReview = await _dbContext.ProductReviews.FirstOrDefaultAsync(r =>
            r.ProductId == 2 && r.UserId == "user1"
        );
        savedReview.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateReview_ShouldReturnFailure_WhenUserAlreadyReviewedProduct()
    {
        // Product 1 already has reviews from user1 (as set up in ReviewTestHelper)
        var reviewDto = new CreateReviewRequestDto
        {
            ProductId = 1,
            Rating = 4,
            Review = "This is another test review",
        };

        var command = new CreateReview.Command { ReviewDto = reviewDto };
        var handler = new CreateReview.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.ShouldContain("already reviewed");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task CreateReview_ShouldReturnFailure_WhenUserHasNotPurchasedProduct()
    {
        // Setup a new product that the user hasn't purchased
        var newProduct = new Product
        {
            Id = 3,
            Name = "Product 3",
            Description = "Description 3",
            RegularPrice = 300,
            Quantity = 30,
            Active = true,
            Length = 30,
            Width = 30,
            Height = 30,
            Weight = 30,
            ShopId = "shop1",
        };

        _dbContext.Products.Add(newProduct);
        await _dbContext.SaveChangesAsync();

        // Arrange review for the new product
        var reviewDto = new CreateReviewRequestDto
        {
            ProductId = 3, // Product that user hasn't purchased
            Rating = 4,
            Review = "This is a test review",
        };

        var command = new CreateReview.Command { ReviewDto = reviewDto };
        var handler = new CreateReview.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.ShouldContain("You can only review products you have purchased");
        result.Code.ShouldBe(400);
    }
}
