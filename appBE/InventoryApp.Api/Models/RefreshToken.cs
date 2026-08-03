namespace InventoryApp.Api.Models; // Перевір, щоб namespace збігався з твоїм проектом

public class RefreshToken
{
    public int Id { get; set; }
    
    // Сам довгий випадковий рядок "паспорта"
    public string Token { get; set; } = string.Empty;
    
    // Кому він належить ("admin" або "user")
    public string Username { get; set; } = string.Empty;
    
    // Коли закінчується його термін дії (наприклад, через 7 днів)
    public DateTime ExpiryDate { get; set; }
    
    // Якщо токен вкрадуть, ми можемо його "спалити" (анулювати)
    public bool IsRevoked { get; set; }
    
    // Зручна властивість, яка сама рахує, чи токен ще дійсний
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;
}