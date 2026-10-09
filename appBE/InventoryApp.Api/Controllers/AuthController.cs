using InventoryApp.Api.DTOs;
using InventoryApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        // 1. Validate credentials against hashed passwords in the database
        var user = await authService.ValidateCredentialsAsync(request.Username, request.Password);

        if (user == null)
            return Unauthorized(new { Message = "Wrong username or password" });

        // 2. Generate JWT access token from the authenticated user
        var tokenString = authService.GenerateAccessToken(user);

        // 3. Generate refresh token and store in DB
        var refreshToken = await authService.GenerateRefreshTokenAsync(user.Username);

        // 4. Set refresh token in an HttpOnly cookie (not accessible from JS)
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = refreshToken.ExpiryDate,
            SameSite = SameSiteMode.Lax
        };
        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

        return Ok(new { Token = tokenString });
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> Refresh()
    {
        var refreshTokenValue = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshTokenValue))
            return Unauthorized(new { Message = "Refresh token is missing" });

        // 1. Validate the refresh token exists and is still active
        var storedToken = await authService.GetActiveRefreshTokenAsync(refreshTokenValue);

        if (storedToken == null)
            return Unauthorized(new { Message = "Invalid or expired refresh token" });

        // 2. Look up the user to get their current role from the database
        var user = await authService.GetUserByUsernameAsync(storedToken.Username);

        if (user == null)
            return Unauthorized(new { Message = "User no longer exists" });

        // 3. Issue a fresh access token
        var tokenString = authService.GenerateAccessToken(user);

        return Ok(new { Token = tokenString });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshTokenValue = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            await authService.RevokeRefreshTokenAsync(refreshTokenValue);
        }

        Response.Cookies.Delete("refreshToken");
        return Ok(new { Message = "Logged out successfully" });
    }
}