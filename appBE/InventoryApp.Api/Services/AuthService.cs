using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InventoryApp.Api.Data;
using InventoryApp.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InventoryApp.Api.Services;

public interface IAuthService
{
    Task<User?> ValidateCredentialsAsync(string username, string password);
    string GenerateAccessToken(User user);
    Task<RefreshToken> GenerateRefreshTokenAsync(string username);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task<User?> GetUserByUsernameAsync(string username);
}

public class AuthService(AppDbContext dbContext, IConfiguration configuration) : IAuthService
{
    /// <summary>
    /// Validates username/password against hashed passwords in the database.
    /// Returns the User if valid, null otherwise.
    /// </summary>
    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        // BCrypt.Verify compares plain-text password against the stored hash
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }

    /// <summary>
    /// Creates a short-lived JWT access token containing user claims.
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates a cryptographically random refresh token and persists it to the database.
    /// </summary>
    public async Task<RefreshToken> GenerateRefreshTokenAsync(string username)
    {
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            Username = username,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        return refreshToken;
    }

    /// <summary>
    /// Finds a non-revoked, non-expired refresh token in the database.
    /// </summary>
    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token)
    {
        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (storedToken == null || !storedToken.IsActive)
            return null;

        return storedToken;
    }

    /// <summary>
    /// Marks a refresh token as revoked so it can no longer be used.
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string token)
    {
        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}

