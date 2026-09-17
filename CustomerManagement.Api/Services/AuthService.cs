using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerManagement.Api.Data;
using CustomerManagement.Api.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CustomerManagement.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<object> _passwordHasher = new();

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string?> LoginAsync(LoginRequestDto request)
    {
        var username = request.Username.Trim();

        var admin = await _context.Admins
            .FirstOrDefaultAsync(x => x.Username == username);

        if (admin == null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(
            new object(),
            admin.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}