using CustomerManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Admin> Admins => Set<Admin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasIndex(x => x.CustomerCode)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasIndex(x => x.PhoneNumber)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Admin>()
            .HasIndex(x => x.Username)
            .IsUnique();
    }
}