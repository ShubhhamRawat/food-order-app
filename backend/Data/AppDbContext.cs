using FoodOrderApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MenuItem>()
            .Property(item => item.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem
            {
                Id = 1,
                Name = "Margherita Pizza",
                Description = "Classic pizza with tomato, mozzarella, and basil.",
                Price = 299.00m,
                ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002"
            },
            new MenuItem
            {
                Id = 2,
                Name = "Classic Burger",
                Description = "Grilled patty, lettuce, tomato, and house sauce.",
                Price = 249.00m,
                ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd"
            },
            new MenuItem
            {
                Id = 3,
                Name = "French Fries",
                Description = "Crispy salted potato fries.",
                Price = 129.00m,
                ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877"
            }
        );

        modelBuilder.Entity<Order>()
    .Property(order => order.TotalAmount)
    .HasPrecision(10, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(orderItem => orderItem.UnitPrice)
            .HasPrecision(10, 2);
    }
}