using FoodOrderApi.Controllers;
using FoodOrderApi.DTOs;
using FoodOrderApi.Models;
using FoodOrderApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FoodOrderApi.Tests.OrderController;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _controller = new OrdersController(_mockOrderService.Object);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
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

        var createdOrder = new Order
        {
            Id = 42,
            CustomerName = "Shubham Rawat",
            DeliveryAddress = "Dehradun, India",
            PhoneNumber = "+91-8279495642",
            TotalAmount = 598.00m,
            Status = OrderStatus.Received
        };

        _mockOrderService
            .Setup(s => s.CreateOrderAsync(request))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(OrdersController.GetOrderById), createdResult.ActionName);
        Assert.Equal(42, createdResult.RouteValues?["id"]);
        Assert.Equal(createdOrder, createdResult.Value);
    }

    [Fact]
    public async Task CreateOrder_InvalidItems_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "Shubham Rawat",
            Items = new List<CreateOrderItemRequest>()
        };

        _mockOrderService
            .Setup(s => s.CreateOrderAsync(request))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("One or more menu items do not exist.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetOrderById_ExistingOrder_ReturnsOkWithOrder()
    {
        // Arrange
        var order = new Order { Id = 10, CustomerName = "Test" };
        _mockOrderService
            .Setup(s => s.GetOrderByIdAsync(10))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.GetOrderById(10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(order, okResult.Value);
    }

    [Fact]
    public async Task GetOrderById_NotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _mockOrderService
            .Setup(s => s.GetOrderByIdAsync(999))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _controller.GetOrderById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateOrderStatus_ValidStatus_ReturnsOkWithUpdatedOrder()
    {
        // Arrange
        var updatedOrder = new Order { Id = 5, Status = OrderStatus.OutForDelivery };
        _mockOrderService
            .Setup(s => s.UpdateOrderStatusAsync(5, OrderStatus.OutForDelivery))
            .ReturnsAsync(updatedOrder);

        var request = new UpdateOrderStatusRequest { Status = OrderStatus.OutForDelivery };

        // Act
        var result = await _controller.UpdateOrderStatus(5, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updatedOrder, okResult.Value);
    }

    [Fact]
    public async Task UpdateOrderStatus_MissingStatus_ReturnsBadRequest()
    {
        // Arrange
        var request = new UpdateOrderStatusRequest { Status = null };

        // Act
        var result = await _controller.UpdateOrderStatus(5, request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Status is required.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateOrderStatus_OrderNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateOrderStatusRequest { Status = OrderStatus.Delivered };
        _mockOrderService
            .Setup(s => s.UpdateOrderStatusAsync(99, OrderStatus.Delivered))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _controller.UpdateOrderStatus(99, request);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
