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

public class GetProductByIdTests
{
    private readonly IMapper _mapper = ProductTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IPhotoService _photoService = A.Fake<IPhotoService>();
    private readonly TestAppDbContext _dbContext = ProductTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task GetProductByIds_ShouldReturnProducts()
    {
        var query = new GetProductById.Query { Id = 1 };
        var handler = new GetProductById.Handler(_dbContext, _mapper, _userAccessor);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.IsType<ProductResponseDto>(result.Value);
    }
}
