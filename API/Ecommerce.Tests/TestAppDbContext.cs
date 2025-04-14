using Ecommerce.Domain;
using Ecommerce.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests;

public class TestAppDbContext(DbContextOptions options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().Property(u => u.DisplayName).HasMaxLength(255);
        modelBuilder.Entity<User>().Property(u => u.ImageUrl).HasMaxLength(255);
        modelBuilder.Entity<User>().Property(u => u.Address).HasMaxLength(255);

        modelBuilder.Entity<Product>().Property(p => p.Name).HasMaxLength(255);
        modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(20000);
        modelBuilder.Entity<Product>().Property(p => p.ShopId).HasMaxLength(36);
        modelBuilder.Entity<Product>().Ignore(p => p.SearchVector);

        modelBuilder.Entity<ProductPhoto>().Property(p => p.Key).HasMaxLength(255);
        modelBuilder.Entity<ProductPhoto>().Property(p => p.ProductId).HasMaxLength(36);

        modelBuilder.Entity<Category>().Property(c => c.Name).HasMaxLength(255);

        modelBuilder.Entity<Subcategory>().Property(c => c.Name).HasMaxLength(255);

        modelBuilder
            .Entity<Product>()
            .HasMany(p => p.Subcategories)
            .WithMany(c => c.Products)
            .UsingEntity<ProductSubcategory>();

        modelBuilder.Entity<Coupon>().Property(c => c.Code).HasMaxLength(255);
        modelBuilder.Entity<Coupon>().Property(c => c.Description).HasMaxLength(1000);

        modelBuilder.Entity<SalesOrder>().Property(so => so.UserId).HasMaxLength(36);
        modelBuilder.Entity<SalesOrder>().Property(so => so.ShippingName).HasMaxLength(255);
        modelBuilder.Entity<SalesOrder>().Property(so => so.ShippingPhone).HasMaxLength(20);
        modelBuilder.Entity<SalesOrder>().Property(so => so.ShippingAddress).HasMaxLength(255);
        modelBuilder.Entity<SalesOrder>().Property(so => so.ShippingOrderCode).HasMaxLength(36);

        modelBuilder.Entity<OrderProduct>().Property(op => op.Name).HasMaxLength(255);

        modelBuilder.Entity<CartItem>().Property(ci => ci.UserId).HasMaxLength(36);
        modelBuilder.Entity<CartItem>().HasKey(ci => new { ci.UserId, ci.ProductId });

        modelBuilder.Entity<ProductReview>().Property(pr => pr.UserId).HasMaxLength(36);
        modelBuilder.Entity<ProductReview>().Property(pr => pr.Review).HasMaxLength(1000);

        modelBuilder.Entity<Payment>().Property(p => p.Description).HasMaxLength(1000);
        modelBuilder.Entity<Payment>().Property(p => p.PaymentMethod).HasMaxLength(255);
        modelBuilder.Entity<Payment>().Property(p => p.ResponseCode).HasMaxLength(100);
        modelBuilder.Entity<Payment>().Property(p => p.ResponseDescription).HasMaxLength(255);
        modelBuilder.Entity<Payment>().Property(p => p.TransactionCode).HasMaxLength(100);
        modelBuilder.Entity<Payment>().Property(p => p.TransactionDescription).HasMaxLength(255);
        modelBuilder.Entity<Payment>().Property(p => p.BankCode).HasMaxLength(100);
        modelBuilder.Entity<Payment>().Property(p => p.BankTransactionId).HasMaxLength(100);
        modelBuilder.Entity<Payment>().Property(p => p.VnpayTransactionId).HasMaxLength(100);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        AddTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker
            .Entries()
            .Where(x =>
                x is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified }
            );

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((BaseEntity)entity.Entity).CreatedAt = now;
            }

            ((BaseEntity)entity.Entity).UpdatedAt = now;
        }
    }
}
