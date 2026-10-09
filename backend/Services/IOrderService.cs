using FoodOrderApi.DTOs;
using FoodOrderApi.Models;

namespace FoodOrderApi.Services;

public interface IOrderService
{
    Task<Order?> CreateOrderAsync(CreateOrderRequest request);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<List<Order>> GetAllOrdersAsync();
    Task<Order?> UpdateOrderStatusAsync(int id, OrderStatus newStatus);
}
