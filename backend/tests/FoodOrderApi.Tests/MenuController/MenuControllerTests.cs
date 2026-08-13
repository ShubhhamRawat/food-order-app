using FoodOrderApi.Controllers;
using FoodOrderApi.Data;
using FoodOrderApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderApi.Tests;

public class MenuControllerTests
{
    [Fact]
    public async Task GetMenu_ReturnsAllMenuItems()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);

        dbContext.MenuItems.AddRange(
            new MenuItem
            {
                Id = 1,
                Name = "Pizza",
                Description = "Test pizza",
                Price = 299.00m,
                ImageUrl = "pizza.jpg"
            },
            new MenuItem
            {
                Id = 2,
                Name = "Burger",
                Description = "Test burger",
                Price = 249.00m,
                ImageUrl = "burger.jpg"
            }
        );

        await dbContext.SaveChangesAsync();

        var controller = new MenuController(dbContext);

        var result = await controller.GetMenu();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var menuItems = Assert.IsType<List<MenuItem>>(okResult.Value);

        Assert.Equal(2, menuItems.Count);
        Assert.Equal("Pizza", menuItems[0].Name);
    }
}