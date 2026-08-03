
namespace InventoryApp.Api.DTOs;

public record CreateItemDto(string Name, decimal Price, bool IsAvailable, int CategoryId, string? ImageUrl);
// Цей об'єкт ми будемо віддавати фронтенду. Тут немає нескінченних списків.
public record ItemResponseDto(int Id, string Name, decimal Price, bool IsAvailable, string CategoryName, string? ImageUrl);