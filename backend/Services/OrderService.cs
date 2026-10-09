using FoodOrderApi.Data;
using FoodOrderApi.DTOs;
using FoodOrderApi.Hubs;
using FoodOrderApi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderApi.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly IHubContext<OrderHub> _hubContext;

    public OrderService(AppDbContext dbContext, IHubContext<OrderHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    public async Task<Order?> CreateOrderAsync(CreateOrderRequest request)
    {
        var menuItemIds = request.Items
            .Select(item => item.MenuItemId)
            .Distinct()
            .ToList();

        var menuItems = await _dbContext.MenuItems
            .Where(menuItem => menuItemIds.Contains(menuItem.Id))
            .ToListAsync();

        if (menuItems.Count != menuItemIds.Count)
        {
            return null; // One or more items not found
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            DeliveryAddress = request.DeliveryAddress,
            PhoneNumber = request.PhoneNumber,
            Status = OrderStatus.Received,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var requestedItem in request.Items)
        {
            var menuItem = menuItems.Single(item => item.Id == requestedItem.MenuItemId);

            order.Items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = requestedItem.Quantity,
                UnitPrice = menuItem.Price
            });
        }

        order.TotalAmount = order.Items.Sum(item => item.UnitPrice * item.Quantity);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _dbContext.Orders
            .Include(order => order.Items)
            .ThenInclude(item => item.MenuItem)
            .FirstOrDefaultAsync(order => order.Id == id);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _dbContext.Orders
            .Include(order => order.Items)
            .ThenInclude(item => item.MenuItem)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Order?> UpdateOrderStatusAsync(int id, OrderStatus newStatus)
    {
        var order = await _dbContext.Orders.FindAsync(id);

        if (order is null)
        {
            return null;
        }

        order.Status = newStatus;
        await _dbContext.SaveChangesAsync();

        // Broadcast real-time status update to subscribed clients via SignalR
        await _hubContext.Clients
            .Group($"order-{id}")
            .SendAsync("OrderStatusUpdated", new
            {
                orderId = id,
                status = order.Status.ToString()
            });

        return order;
    }
}
