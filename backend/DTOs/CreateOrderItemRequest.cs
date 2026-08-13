using System.ComponentModel.DataAnnotations;

namespace FoodOrderApi.DTOs;

public class CreateOrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int MenuItemId { get; set; }

    [Range(1, 20)]
    public int Quantity { get; set; }
}