namespace InventoryApp.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;

    // BCrypt hash — never store plain-text passwords
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer"; // "Admin" or "Customer"
}

