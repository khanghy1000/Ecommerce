using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Locations.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Locations.Queries;

public class ListDistrict
{
    public class Query : IRequest<Result<List<DistrictResponseDto>>>
    {
        public int? ProvinceId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<DistrictResponseDto>>>
    {
        public async Task<Result<List<DistrictResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var query = dbContext.Districts.AsQueryable();
            if (request.ProvinceId.HasValue)
            {
                query = query.Where(x => x.ProvinceId == request.ProvinceId);
            }

            var districts = await query
                .OrderBy(x => x.Name)
                .ProjectTo<DistrictResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<DistrictResponseDto>>.Success(districts);
        }
    }
}
