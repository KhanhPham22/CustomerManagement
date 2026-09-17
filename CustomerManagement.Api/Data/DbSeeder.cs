using CustomerManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Admins.AnyAsync())
            return;

        var hasher = new PasswordHasher<object>();

        var admin = new Admin
        {
            Username = "admin",
            PasswordHash = hasher.HashPassword(
                new object(),
                "Admin@123")
        };

        context.Admins.Add(admin);

        await context.SaveChangesAsync();
    }
}