using Ecommerce.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Persistence;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Subcategory> Subcategories { get; set; }
    public DbSet<ProductSubcategory> ProductSubcategories { get; set; }
    public DbSet<ProductPhoto> ProductPhotos { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Ward> Wards { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ProductDiscount> ProductDiscounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("fuzzystrmatch");

        modelBuilder.Entity<User>().Property(u => u.DisplayName).HasMaxLength(255);
        modelBuilder.Entity<User>().Property(u => u.ImageUrl).HasMaxLength(255);
        modelBuilder.Entity<User>().Property(u => u.Address).HasMaxLength(255);

        modelBuilder.Entity<Product>().Property(p => p.Name).HasMaxLength(255);
        modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(20000);
        modelBuilder.Entity<Product>().Property(p => p.ShopId).HasMaxLength(36);
        modelBuilder.Entity<Product>().Property(p => p.Active).HasDefaultValue(true);

        modelBuilder
            .Entity<Product>()
            .HasGeneratedTsVectorColumn(p => p.SearchVector, "vietnamese", p => new { p.Name })
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN");

        modelBuilder.Entity<ProductPhoto>().Property(p => p.Key).HasMaxLength(255);
        modelBuilder.Entity<ProductPhoto>().Property(p => p.ProductId).HasMaxLength(36);

        modelBuilder.Entity<Category>().Property(c => c.Name).HasMaxLength(255);

        modelBuilder.Entity<Subcategory>().Property(c => c.Name).HasMaxLength(255);

        modelBuilder
            .Entity<Product>()
            .HasMany(p => p.Subcategories)
            .WithMany(c => c.Products)
            .UsingEntity<ProductSubcategory>();

        modelBuilder.Entity<Coupon>().Property(c => c.Code).HasMaxLength(50);

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

        modelBuilder.Entity<Coupon>().Property(c => c.UsedCount).HasDefaultValue(0);
        modelBuilder.Entity<Coupon>().Property(c => c.Active).HasDefaultValue(true);
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
