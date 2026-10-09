using FoodOrderApi.Data;
using FoodOrderApi.DTOs;
using FoodOrderApi.Hubs;
using FoodOrderApi.Models;
using FoodOrderApi.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FoodOrderApi.Tests.Services;

public class OrderServiceTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidItems_CalculatesTotalAndSaves()
    {
        // Arrange
        await using var dbContext = CreateInMemoryDbContext();
        dbContext.MenuItems.Add(new MenuItem
        {
            Id = 1,
            Name = "Margherita Pizza",
            Price = 299.00m,
            Description = "Delicious",
            ImageUrl = "img.png"
        });
        await dbContext.SaveChangesAsync();

        var mockHubContext = new Mock<IHubContext<OrderHub>>();
        var orderService = new OrderService(dbContext, mockHubContext.Object);

        var request = new CreateOrderRequest
        {
            CustomerName = "Shubham Rawat",
            DeliveryAddress = "Dehradun, India",
            PhoneNumber = "+91-8279495642",
            Items = new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest { MenuItemId = 1, Quantity = 2 }
            }
        };

        // Act
        var order = await orderService.CreateOrderAsync(request);

        // Assert
        Assert.NotNull(order);
        Assert.Equal("Shubham Rawat", order.CustomerName);
        Assert.Equal(OrderStatus.Received, order.Status);
        Assert.Equal(598.00m, order.TotalAmount); // 299 * 2
        Assert.Single(order.Items);
    }

    [Fact]
    public async Task CreateOrderAsync_InvalidMenuItemId_ReturnsNull()
    {
        // Arrange
        await using var dbContext = CreateInMemoryDbContext();
        var mockHubContext = new Mock<IHubContext<OrderHub>>();
        var orderService = new OrderService(dbContext, mockHubContext.Object);

        var request = new CreateOrderRequest
        {
            CustomerName = "Shubham Rawat",
            DeliveryAddress = "Dehradun, India",
            PhoneNumber = "+91-8279495642",
            Items = new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest { MenuItemId = 999, Quantity = 1 } // Non-existent
            }
        };

        // Act
        var order = await orderService.CreateOrderAsync(request);

        // Assert
        Assert.Null(order);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ValidId_UpdatesStatusAndBroadcastsViaSignalR()
    {
        // Arrange
        await using var dbContext = CreateInMemoryDbContext();
        var existingOrder = new Order
        {
            Id = 101,
            CustomerName = "Test User",
            DeliveryAddress = "Test Address",
            PhoneNumber = "1234567890",
            Status = OrderStatus.Received,
            TotalAmount = 299.00m,
            CreatedAtUtc = DateTime.UtcNow
        };
        dbContext.Orders.Add(existingOrder);
        await dbContext.SaveChangesAsync();

        var mockHubContext = new Mock<IHubContext<OrderHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group("order-101")).Returns(mockClientProxy.Object);

        var orderService = new OrderService(dbContext, mockHubContext.Object);

        // Act
        var updatedOrder = await orderService.UpdateOrderStatusAsync(101, OrderStatus.Preparing);

        // Assert
        Assert.NotNull(updatedOrder);
        Assert.Equal(OrderStatus.Preparing, updatedOrder.Status);

        // Verify that SignalR broadcasted the event to group "order-101"
        mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "OrderStatusUpdated",
                It.Is<object[]>(args => args.Length > 0),
                default),
            Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        await using var dbContext = CreateInMemoryDbContext();
        var mockHubContext = new Mock<IHubContext<OrderHub>>();
        var orderService = new OrderService(dbContext, mockHubContext.Object);

        // Act
        var result = await orderService.UpdateOrderStatusAsync(999, OrderStatus.Delivered);

        // Assert
        Assert.Null(result);
    }
}
