using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Locations.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Locations.Queries;

public class ListProvinces
{
    public class Query : IRequest<Result<List<ProvinceDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<ProvinceDto>>>
    {
        public async Task<Result<List<ProvinceDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var provinces = await dbContext
                .Provinces.OrderBy(x => x.Name)
                .ProjectTo<ProvinceDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<ProvinceDto>>.Success(provinces);
        }
    }
}
