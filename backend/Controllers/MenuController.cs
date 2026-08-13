using FoodOrderApi.Data;
using FoodOrderApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public MenuController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<MenuItem>>> GetMenu()
    {
        var menuItems = await _dbContext.MenuItems
            .AsNoTracking()
            .ToListAsync();

        return Ok(menuItems);
    }
}