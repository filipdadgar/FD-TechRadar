using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace TechRadar.Api.Controllers;

[ApiController]
[Route("admin/auth")]
public class AuthController(IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var expectedUsername = config["ADMIN_USERNAME"] ?? string.Empty;
        var expectedPassword = config["ADMIN_PASSWORD"] ?? string.Empty;

        if (request.Username != expectedUsername || request.Password != expectedPassword)
            return Unauthorized(new { error = "Invalid credentials." });

        var secret = config["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTimeOffset.UtcNow.AddHours(24);

        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Name, request.Username), new Claim(ClaimTypes.Role, "Admin")],
            expires: expiry.UtcDateTime,
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt = expiry
        });
    }

    public record LoginRequest(string Username, string Password);
}
