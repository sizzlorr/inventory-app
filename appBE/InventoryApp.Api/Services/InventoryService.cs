using InventoryApp.Api.Controllers;
using InventoryApp.Api.Data;
using InventoryApp.Api.Models;
using InventoryApp.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Api.Services;

public interface IInventoryService
{
    Task<IEnumerable<ItemResponseDto>> GetAllItemsAsync(
        string? searchText = null,
        int? categoryId = null,
        int page = 1,
        int pageSize = 10);
    Task<InventoryItem?> GetItemByIdAsync(int id);
    Task<ItemResponseDto> CreateItemAsync(CreateItemDto dto);
    Task<InventoryItem?> UpdateItemAsync(int id, CreateItemDto dto);
    Task<bool> DeleteItemAsync(int id);
}

public class InventoryService(AppDbContext context) : IInventoryService
{
    public async Task<IEnumerable<ItemResponseDto>> GetAllItemsAsync(
        string? searchText = null,
        int? categoryId = null,
        int page = 1,
        int pageSize = 10)
    {
        var query = context.InventoryItems
            .Include(i => i.Category)
            .AsQueryable();
        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(i => i.Name.ToLower().Contains(searchText.ToLower()));
        }
        
        var skip = (page - 1) * pageSize;
        
        query = query.Skip(skip).Take(pageSize);
        
        var items = await query.ToListAsync();

        var dtoList = items.Select(item => new ItemResponseDto(
            item.Id,
            item.Name,
            item.Price,
            item.IsAvailable,
            item.Category != null ? item.Category.Name : "No Category",
            item.ImageUrl
        ));
        
        return dtoList;
    }
    
    public async Task<InventoryItem?> GetItemByIdAsync(int id)
    {
        return await context.InventoryItems.FindAsync(id);
    }

    public async Task<ItemResponseDto> CreateItemAsync(CreateItemDto dto)
    {
        var newItem = new InventoryItem
        {
            Name = dto.Name,
            Price = dto.Price,
            IsAvailable = dto.IsAvailable,
            CategoryId = dto.CategoryId,
            ImageUrl = dto.ImageUrl
        };
        
        context.InventoryItems.Add(newItem);
        await context.SaveChangesAsync();

        // Load the category name for the response DTO
        var categoryName = await context.Categories
            .Where(c => c.Id == newItem.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync() ?? "No Category";

        return new ItemResponseDto(
            newItem.Id,
            newItem.Name,
            newItem.Price,
            newItem.IsAvailable,
            categoryName,
            newItem.ImageUrl);
    }
    
    public async Task<InventoryItem?> UpdateItemAsync(int id, CreateItemDto dto)
    {
        var itemToUpdate = await context.InventoryItems.FindAsync(id);

        if (itemToUpdate == null) return null;

        itemToUpdate.Name = dto.Name;
        itemToUpdate.Price = dto.Price;
        itemToUpdate.IsAvailable = dto.IsAvailable;
        itemToUpdate.CategoryId = dto.CategoryId;
        itemToUpdate.ImageUrl = dto.ImageUrl;
        
        await context.SaveChangesAsync();
        
        return itemToUpdate;
    }
    
    public async Task<bool> DeleteItemAsync(int id)
    {
        var itemToDelete = await context.InventoryItems.FindAsync(id);

        if (itemToDelete == null) return false;

        context.InventoryItems.Remove(itemToDelete);
        
        await context.SaveChangesAsync();

        return true;
    }
}