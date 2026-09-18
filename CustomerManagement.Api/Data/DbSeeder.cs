using CustomerManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Api.Data;

// Seeds initial data into the database.
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Skip seeding if an admin already exists.
        if (await context.Admins.AnyAsync())
            return;

        // Creates a password hasher to securely hash the admin password.
        var hasher = new PasswordHasher<object>();

        // Creates the default admin account with a hashed password.
        var admin = new Admin
        {
            Username = "admin",
            PasswordHash = hasher.HashPassword(
                new object(),
                "Admin@123")
        };

        // Add the default admin to the database.
        context.Admins.Add(admin);

        // Save the admin account to the database.
        await context.SaveChangesAsync();
    }
}