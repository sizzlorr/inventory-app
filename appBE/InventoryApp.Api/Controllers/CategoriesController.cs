using InventoryApp.Api.Data;
using InventoryApp.Api.DTOs;
using InventoryApp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(AppDbContext context): ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] string categoryName)
    {
        var category = new Category { Name = categoryName };

        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return Ok(category);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await context.Categories.ToListAsync());
    }
}