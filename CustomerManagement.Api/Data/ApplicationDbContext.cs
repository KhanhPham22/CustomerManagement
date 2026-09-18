using CustomerManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Api.Data;

// Represents the database context and manages database access through Entity Framework Core.
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Represents the Customers table in the database.
    public DbSet<Customer> Customers => Set<Customer>();

    // Represents the Admins table in the database.
    public DbSet<Admin> Admins => Set<Admin>();

    // Configures database rules and constraints for the entities.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CustomerCode must be unique.
        modelBuilder.Entity<Customer>()
            .HasIndex(x => x.CustomerCode)
            .IsUnique();

        // PhoneNumber must be unique.
        modelBuilder.Entity<Customer>()
            .HasIndex(x => x.PhoneNumber)
            .IsUnique();

        // Email must be unique.
        modelBuilder.Entity<Customer>()
            .HasIndex(x => x.Email)
            .IsUnique();

        // Admin username must be unique.
        modelBuilder.Entity<Admin>()
            .HasIndex(x => x.Username)
            .IsUnique();
    }
}