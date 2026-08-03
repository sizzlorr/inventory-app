namespace InventoryApp.Api.Models;

public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    // 1. Зовнішній ключ (Foreign Key) - число, яке зберігатиметься в таблиці
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }

    // 2. Навігаційна властивість - посилання на сам об'єкт категорії
    // Символ "?" означає, що під час створення об'єкта ми можемо не передавати сюди всю категорію одразу
    public Category? Category { get; set; }
}
