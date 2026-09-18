using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerManagement.Api.Data;
using CustomerManagement.Api.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CustomerManagement.Api.Services;

// Handles authentication logic and JWT token generation.
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
        // Find the admin account by username.
        var username = request.Username.Trim();

        var admin = await _context.Admins
            .FirstOrDefaultAsync(x => x.Username == username);

        // Return null if the username does not exist.
        if (admin == null)
            return null;

        // Verify the provided password against the stored password hash.
        var result = _passwordHasher.VerifyHashedPassword(
            new object(),
            admin.PasswordHash,
            request.Password);

        // Return null if the password is incorrect.
        if (result == PasswordVerificationResult.Failed)
            return null;

        // Add user information to the JWT token.
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString())
        };

        // Get the secret key used to sign the JWT.
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        // Define how the JWT will be digitally signed.
        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        // Create a JWT token that expires after 8 hours.
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        // Convert the JWT token into a string and return it.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}