using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Application.Reviews.Commands;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Application.Reviews.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class ReviewsController : BaseApiController
{
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewResponseDto>> GetReview(int id)
    {
        var result = await Mediator.Send(new GetReviewById.Query { ReviewId = id });
        return HandleResult(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedList<ReviewResponseDto>>> GetProduct(
        int? productId,
        string? userId,
        int pageSize = 20,
        int pageNumber = 1
    )
    {
        var result = await Mediator.Send(
            new ListProductReviews.Query
            {
                ProductId = productId,
                UserId = userId,
                PageSize = pageSize,
                PageNumber = pageNumber,
            }
        );
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<ReviewResponseDto>> CreateReview(
        CreateReviewRequestDto reviewDto
    )
    {
        var result = await Mediator.Send(new CreateReview.Command { ReviewDto = reviewDto });
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Buyer", Policy = "IsReviewOwner")]
    public async Task<ActionResult<ReviewResponseDto>> UpdateReview(
        int id,
        UpdateReviewRequestDto reviewDto
    )
    {
        var result = await Mediator.Send(
            new UpdateReview.Command { ReviewId = id, ReviewDto = reviewDto }
        );
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Buyer", Policy = "IsReviewOwner")]
    public async Task<ActionResult<Unit>> DeleteReview(int id)
    {
        var result = await Mediator.Send(new DeleteReview.Command { ReviewId = id });
        return HandleResult(result);
    }
}
