using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using InventoryApp.Api.Services;
using InventoryApp.Api.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace InventoryApp.Api.Controllers;


// 1. Вказує фреймворку, що цей клас є API-контролером (вмикає автоматичну валідацію помилок)
[ApiController] 
// 2. Базовий шлях. [controller] автоматично підставить назву класу без слова "Controller". 
// Тобто шлях буде: /api/inventory
[Route("api/[controller]")] 
public class InventoryController(IInventoryService inventoryService, IValidator<CreateItemDto> validator): ControllerBase // 3. Успадковуємо базовий функціонал контролера
{
    [HttpGet] 
    public async Task<IActionResult> GetAllItems(
        [FromQuery] string? search, 
        [FromQuery] int? categoryId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        if (pageSize > 50) pageSize = 50; 
        if (page < 1) page = 1;

        var items = await inventoryService.GetAllItemsAsync(
            search,
            categoryId,
            page,
            pageSize);

        return Ok(items); 
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetItemById(int id)
    {
        var item = await inventoryService.GetItemByIdAsync(id);
        if (item == null) return NotFound(new { Error = "Item not found" });
        
        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto newItem)
    {
        var validationResult = await validator.ValidateAsync(newItem);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var createdItem = await inventoryService.CreateItemAsync(newItem);
        
        return Created($"/api/inventory/{createdItem.Id}", createdItem);
    }
    
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] CreateItemDto updatedItem)
    {
        var validationResult = await validator.ValidateAsync(updatedItem);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var item = await inventoryService.UpdateItemAsync(id, updatedItem);
        if (item == null) return NotFound(new { Error = "Item not found" });

        return Ok(item);
    }
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var success = await inventoryService.DeleteItemAsync(id);
        if (!success) return NotFound(new { Error = "Item not found" });

        return NoContent();
    }
}