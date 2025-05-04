We use ASP.NET to make API for our e-commerce website. This project uses .NET 9 and C#.

Library used:

-   Entity Framework for ORM.
-   MediatR for business layer.
-   Fluent Validation to validate mediatR command/query requests.
-   AutoMapper to map objects.
-   XUnit and FakeItEasy to write unit tests.

Business layer structure:

-   Project: Ecommerce.Application.
-   Related validators, commands, queries, DTOs are grouped together inside a folder (example: Products folder).
-   Fluent Validation validators are in Validators folder.
-   Commands are in Commands folder.
-   Queries are in Queries folder.
-   DTOs are in DTOs folder.

Each mediatR command/query result must wrapped in Result class in Ecommerce.Application/Core.Result.cs file.

Example of ListProduct query in Ecommerce.Application/Products/Queries folder:

```C#
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public static class ListProducts
{
    public class Query : IRequest<Result<List<ProductDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<ProductDto>>>
    {
        public async Task<Result<List<ProductDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var products = await dbContext
                .Products.ProjectTo<ProductDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            return Result<List<ProductDto>>.Success(products);
        }
    }
}

```
