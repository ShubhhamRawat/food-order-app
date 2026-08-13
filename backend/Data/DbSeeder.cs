using FoodOrderApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.MenuItems.AnyAsync())
        {
            return;
        }

        dbContext.MenuItems.AddRange(
            new MenuItem
            {
                Name = "Margherita Pizza",
                Description = "Classic pizza with tomato, mozzarella, and basil.",
                Price = 299.00m,
                ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002"
            },
            new MenuItem
            {
                Name = "Classic Burger",
                Description = "Grilled patty, lettuce, tomato, and house sauce.",
                Price = 249.00m,
                ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd"
            },
            new MenuItem
            {
                Name = "French Fries",
                Description = "Crispy salted potato fries.",
                Price = 129.00m,
                ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877"
            }
        );

        await dbContext.SaveChangesAsync();
    }
}