using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Features.Auth;

public class AuthService(AppDbContext db, IConfiguration configuration)
{
    /// <summary>Wrong credentials and inactive users both answer 401 with the same message.</summary>
    public async Task<AuthResponseDto> LoginAsync(LoginDto login)
    {
        var email = login.Email.ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        var expiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user, expiresAt),
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id,
            ExpiresAt = expiresAt,
        };
    }

    private int ExpirationMinutes => int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

    private string GenerateJwtToken(User user, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));
        var role = user.Role.ToString();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role),
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("role", role),
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
