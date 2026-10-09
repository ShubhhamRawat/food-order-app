using Microsoft.AspNetCore.SignalR;

namespace FoodOrderApi.Hubs;

public class OrderHub : Hub
{
    /// <summary>
    /// Adds the current client connection to a room dedicated to a specific order.
    /// This ensures only clients tracking this order receive its status updates.
    /// </summary>
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    /// <summary>
    /// Removes the client connection from the order room.
    /// </summary>
    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }
}
