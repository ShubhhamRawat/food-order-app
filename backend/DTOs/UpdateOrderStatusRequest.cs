using System.ComponentModel.DataAnnotations;
using FoodOrderApi.Models;

namespace FoodOrderApi.DTOs;

public class UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus? Status { get; set; }
}