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

namespace Ecommerce.Tests.Application.Products.Commands;

public class DeleteProductTests
{
    private readonly IMapper _mapper = ProductTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IPhotoService _photoService = A.Fake<IPhotoService>();
    private readonly TestAppDbContext _dbContext = ProductTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task DeleteProduct_ShouldReturnSuccess()
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync();
        var command = new DeleteProduct.Command { Id = product!.Id };
        var handler = new DeleteProduct.Handler(_dbContext);

        var deleteResult = await handler.Handle(command, CancellationToken.None);
        var deletedProduct = await _dbContext.Products.FindAsync(product.Id);

        deleteResult.IsSuccess.ShouldBeTrue();
        deleteResult.Error.ShouldBeNull();
        deletedProduct.ShouldBeNull();
    }
}
