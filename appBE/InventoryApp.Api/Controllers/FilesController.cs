using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")] // Тільки адмін може заливати фото
    // IFormFile - це вбудований інтерфейс для прийому файлів
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        // 1. Перевіряємо, чи взагалі прислали файл
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Message = "Файл не вибрано" });
        }

        // 2. Визначаємо шлях до папки wwwroot/images
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        
        // Якщо такої папки ще немає — створюємо її
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // 3. Генеруємо унікальне ім'я файлу (щоб дві різні фотки "boxart.jpg" не перезаписали одна одну)
        // Guid.NewGuid() створює випадковий набір символів
        // Замінюємо всі пробіли на дефіси, щоб уникнути проблем з URL
        var cleanFileName = file.FileName.Replace(" ", "-");
        var uniqueFileName = Guid.NewGuid().ToString() + "_" + cleanFileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // 4. Зберігаємо файл на жорсткий диск сервера
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 5. Повертаємо відносний шлях, який фронтенд зможе підставити в <img src="...">
        var imageUrl = $"/images/{uniqueFileName}";
        
        return Ok(new { Url = imageUrl });
    }
}