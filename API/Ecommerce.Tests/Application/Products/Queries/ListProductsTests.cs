using Amazon.S3.Model;
using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Products.Helpers;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Products.Queries;

public class ListProductsTests
{
    private readonly IMapper _mapper = ProductTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IPhotoService _photoService = A.Fake<IPhotoService>();
    private readonly TestAppDbContext _dbContext = ProductTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task ListProducts_ShouldReturnPagedListOfProducts()
    {
        var query = new ListProducts.Query();
        var handler = new ListProducts.Handler(_dbContext, _mapper, _userAccessor);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<PagedList<ProductResponseDto>>();
    }

    [Fact]
    public async Task ListProducts_ShouldReturnFilteredProducts()
    {
        var query = new ListProducts.Query
        {
            MinPrice = 100,
            MaxPrice = 110,
            PageSize = 5,
            PageNumber = 1,
        };
        var handler = new ListProducts.Handler(_dbContext, _mapper, _userAccessor);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<PagedList<ProductResponseDto>>();
        result.Value.Items.Count.ShouldBeLessThanOrEqualTo(5);
        result.Value.Items.ShouldAllBe(i =>
            (i.DiscountPrice ?? i.RegularPrice) >= 100 && (i.DiscountPrice ?? i.RegularPrice) <= 110
        );
    }
}
