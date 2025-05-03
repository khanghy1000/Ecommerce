using Ecommerce.Application.Core;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class UpdatePopularProduct
{
    public class Command : IRequest<Result<Unit>> { }

    public class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

            var categories = await dbContext
                .Categories.Include(c => c.Subcategories)
                .ToListAsync(cancellationToken);

            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

            try
            {
                await dbContext.PopularProducts.ExecuteDeleteAsync(cancellationToken);

                var popularProducts = new List<PopularProduct>();

                foreach (var category in categories)
                {
                    var subcategoryIds = category.Subcategories.Select(s => s.Id).ToList();

                    if (subcategoryIds.Count == 0)
                        continue;

                    // Get top 10 popular products in the last 3 days by sales count
                    var popularProductsInCategory = await dbContext
                        .OrderProducts.Include(op => op.Product)
                        .Include(op => op.Product.Subcategories)
                        .Where(op => op.Order.OrderTime >= threeDaysAgo)
                        // .Where(op => op.Order.Status != SalesOrderStatus.Cancelled) // Exclude canceled orders
                        .Where(op =>
                            op.Product.Subcategories.Any(s => subcategoryIds.Contains(s.Id))
                        )
                        .GroupBy(op => op.ProductId)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            SalesCount = g.Sum(op => op.Quantity),
                        })
                        .OrderByDescending(x => x.SalesCount)
                        .Take(10)
                        .ToListAsync(cancellationToken);

                    foreach (var item in popularProductsInCategory)
                    {
                        popularProducts.Add(
                            new PopularProduct
                            {
                                CategoryId = category.Id,
                                ProductId = item.ProductId,
                                SalesCount = item.SalesCount,
                            }
                        );
                    }

                    // If there are less than 10 products in the category, add more products from the category
                    if (popularProductsInCategory.Count < 10)
                    {
                        var existingPopularProductIds = popularProductsInCategory
                            .Select(p => p.ProductId)
                            .ToList();

                        var additionalProducts = await dbContext
                            .Products.Where(p => p.Active)
                            .Where(p => p.Subcategories.Any(s => subcategoryIds.Contains(s.Id)))
                            .Where(p => !existingPopularProductIds.Contains(p.Id))
                            .OrderByDescending(p => p.CreatedAt)
                            .Take(10 - popularProductsInCategory.Count)
                            .Select(p => p.Id)
                            .ToListAsync(cancellationToken);

                        foreach (var productId in additionalProducts)
                        {
                            popularProducts.Add(
                                new PopularProduct
                                {
                                    CategoryId = category.Id,
                                    ProductId = productId,
                                    SalesCount = 0,
                                }
                            );
                        }
                    }
                }

                await dbContext.PopularProducts.AddRangeAsync(popularProducts, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<Unit>.Failure(
                    $"Failed to update popular products: {ex.Message}",
                    500
                );
            }
        }
    }
}
