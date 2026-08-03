using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography; // Додали для генерації випадкових рядків
using System.Text;
using InventoryApp.Api.Data;
using InventoryApp.Api.DTOs;
using InventoryApp.Api.Models; // Твій namespace для моделей
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
// Додали AppDbContext у конструктор
public class AuthController(IConfiguration configuration, AppDbContext dbContext) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, request.Username)
        };

        var adminUser = configuration["AdminSettings:Username"];
        var adminPass = configuration["AdminSettings:Password"];

        if (request.Username == adminUser && request.Password == adminPass)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        else if (request.Username == "user" && request.Password == "user123")
        {
            claims.Add(new Claim(ClaimTypes.Role, "Customer"));
        }
        else
        {
            return Unauthorized(new { Message = "Wrong username or password" });
        }

        // --- ГЕНЕРУЄМО ACCESS TOKEN (ЯК БУЛО) ---
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // --- НОВЕ: ГЕНЕРУЄМО REFRESH TOKEN ---
        
        // 1. Створюємо надійний випадковий рядок (64 байти)
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        var refreshTokenString = Convert.ToBase64String(randomBytes);

        // 2. Зберігаємо його в базу даних
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            Username = request.Username,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // Живе 7 днів
            IsRevoked = false
        };
        
        dbContext.RefreshTokens.Add(refreshToken);
        dbContext.SaveChanges();

        // 3. Кладемо Refresh Token у захищену Cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Забороняє JavaScript на фронтенді красти цей токен
            Expires = refreshToken.ExpiryDate,
            SameSite = SameSiteMode.Lax // Щоб працювало на localhost
        };
        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

        // Повертаємо Access Token у JSON
        return Ok(new { Token = tokenString });
    }

    [HttpPost("refreshToken")]
    public IActionResult Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { Message = "Refresh token is missing" });
        }

        var storedToken = dbContext.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            return Unauthorized(new { Message = "Invalid or expired refresh token" });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, storedToken.Username)
        };

        var adminUser = configuration["AdminSettings:Username"];

        if (storedToken.Username == adminUser)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, "Customer"));
        }
        
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var newToken = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1), 
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(newToken);

        // Віддаємо новий Access-токен
        return Ok(new { Token = tokenString });
    }
    
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var storedToken = dbContext.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                dbContext.SaveChanges();
            }
        }
    
        // Видаляємо куку з браузера
        Response.Cookies.Delete("refreshToken");
        return Ok(new { Message = "Logged out successfully" });
    }
}