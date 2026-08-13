using FoodOrderApi.Data;
using FoodOrderApi.DTOs;
using FoodOrderApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public OrdersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(
        CreateOrderRequest request)
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
            return BadRequest("One or more menu items do not exist.");
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
            var menuItem = menuItems.Single(
                item => item.Id == requestedItem.MenuItemId);

            order.Items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = requestedItem.Quantity,
                UnitPrice = menuItem.Price
            });
        }

        order.TotalAmount = order.Items.Sum(
            item => item.UnitPrice * item.Quantity);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = order.Id },
            order);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetOrderById(int id)
    {
        var order = await _dbContext.Orders
            .Include(order => order.Items)
            .ThenInclude(item => item.MenuItem)
            .FirstOrDefaultAsync(order => order.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<Order>> UpdateOrderStatus(
    int id,
    UpdateOrderStatusRequest request)
    {
        var order = await _dbContext.Orders.FindAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        order.Status = request.Status!.Value;

        await _dbContext.SaveChangesAsync();

        return Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetOrders()
    {
        var orders = await _dbContext.Orders
            .Include(order => order.Items)
            .ThenInclude(item => item.MenuItem)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync();

        return Ok(orders);
    }
}