using Microsoft.EntityFrameworkCore;
using Ecommerce.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Ecommerce.Persistence;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
{
    // TODO: Add reviews, shipping, payment.

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().Property(u => u.FirstName).HasMaxLength(255);
        modelBuilder.Entity<User>().Property(u => u.LastName).HasMaxLength(255);
        modelBuilder.Entity<User>().Property(u => u.ImageUrl).HasMaxLength(255);

        modelBuilder.Entity<Product>().Property(p => p.Name).HasMaxLength(255);
        modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(20000);

        modelBuilder.Entity<Category>().Property(c => c.Name).HasMaxLength(255);
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.InverseParent)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tag>().Property(t => t.Name).HasMaxLength(255);

        modelBuilder.Entity<Coupon>().Property(c => c.Code).HasMaxLength(255);
        modelBuilder.Entity<Coupon>().Property(c => c.Description).HasMaxLength(1000);
        
        modelBuilder.Entity<SalesOrder>().Property(so => so.UserId).HasMaxLength(36);

        modelBuilder.Entity<OrderProduct>().Property(op => op.Name).HasMaxLength(255);

        modelBuilder.Entity<CartItem>().Property(ci => ci.UserId).HasMaxLength(36);
        modelBuilder.Entity<CartItem>().HasKey(ci => new { ci.UserId, ci.ProductId });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        AddTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified });

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