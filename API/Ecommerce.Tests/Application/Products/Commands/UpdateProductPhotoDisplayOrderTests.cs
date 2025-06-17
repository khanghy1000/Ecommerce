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

public class UpdateProductPhotoDisplayOrderTests
{
    private readonly IMapper _mapper = ProductTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IPhotoService _photoService = A.Fake<IPhotoService>();
    private readonly TestAppDbContext _dbContext = ProductTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task UpdateProductPhotoDisplayOrder_ShouldReturnSuccess()
    {
        var product = await _dbContext
            .Products.Include(product => product.Photos)
            .FirstOrDefaultAsync();
        // add photos to product
        var photos = new List<ProductPhoto>
        {
            new ProductPhoto
            {
                Key = "photos/products/1/photo1.jpg",
                DisplayOrder = 1,
                ProductId = product!.Id,
            },
            new ProductPhoto
            {
                Key = "photos/products/1/photo2.jpg",
                DisplayOrder = 2,
                ProductId = product.Id,
            },
            new ProductPhoto
            {
                Key = "photos/products/1/photo3.jpg",
                DisplayOrder = 3,
                ProductId = product.Id,
            },
        };
        product.Photos.Clear();
        _dbContext.ProductPhotos.AddRange(photos);
        await _dbContext.SaveChangesAsync();

        var command = new UpdateProductPhotoDisplayOrder.Command
        {
            ProductId = product.Id,
            PhotoOrders = new List<UpdateProductPhotoDisplayOrderRequestDto>
            {
                new() { Key = "photos/products/1/photo2.jpg", DisplayOrder = 1 },
                new() { Key = "photos/products/1/photo3.jpg", DisplayOrder = 2 },
                new() { Key = "photos/products/1/photo1.jpg", DisplayOrder = 3 },
            },
        };
        var handler = new UpdateProductPhotoDisplayOrder.Handler(_dbContext);

        var result = await handler.Handle(command, CancellationToken.None);
        var updatedPhotos = await _dbContext
            .ProductPhotos.Where(p => p.ProductId == product.Id)
            .ToListAsync();

        result.IsSuccess.ShouldBeTrue();
        updatedPhotos.ShouldNotBeEmpty();
        updatedPhotos.ShouldContain(p =>
            p.Key == "photos/products/1/photo2.jpg" && p.DisplayOrder == 1
        );
        updatedPhotos.ShouldContain(p =>
            p.Key == "photos/products/1/photo3.jpg" && p.DisplayOrder == 2
        );
        updatedPhotos.ShouldContain(p =>
            p.Key == "photos/products/1/photo1.jpg" && p.DisplayOrder == 3
        );
    }
}
