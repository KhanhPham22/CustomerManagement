using CustomerManagement.Api.Data;
using CustomerManagement.Api.DTOs;
using CustomerManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Api.Services;

// Handles customer-related business logic and database operations.
public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all customers with optional search by name or phone number.
    public async Task<List<CustomerDto>> GetCustomersAsync(string? search)
    {
        var query = _context.Customers.AsQueryable();

        // Filter customers when a search keyword is provided.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.PhoneNumber.Contains(search));
        }

        // Return customers as DTOs ordered by ID.
        return await query
            .OrderBy(x => x.Id)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                CustomerCode = x.CustomerCode,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                DateOfBirth = x.DateOfBirth,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    // Get a single customer by ID.
    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        // Return null if the customer does not exist.
        return await _context.Customers
            .Where(x => x.Id == id)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                CustomerCode = x.CustomerCode,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                DateOfBirth = x.DateOfBirth,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    // Create a new customer after validating the input.
    public async Task<CustomerDto> CreateCustomerAsync(CustomerRequestDto request)
    {
        // Clean up input data and validate the date of birth.
        NormalizeRequest(request);

        ValidateDateOfBirth(request.DateOfBirth);

        // Check for duplicate customer code, phone number, and email.
        if (await _context.Customers.AnyAsync(x =>
            x.CustomerCode == request.CustomerCode))
        {
            throw new ArgumentException("Customer code already exists.");
        }

        if (await _context.Customers.AnyAsync(x =>
            x.PhoneNumber == request.PhoneNumber))
        {
            throw new ArgumentException("Phone number already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _context.Customers.AnyAsync(x =>
                x.Email == request.Email))
        {
            throw new ArgumentException("Email already exists.");
        }

        // Create a Customer entity from the request data.
        var customer = new Customer
        {
            CustomerCode = request.CustomerCode,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            IsActive = request.IsActive
        };

        // Save the new customer to the database.
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    // Update an existing customer after validating the input.
    public async Task<CustomerDto?> UpdateCustomerAsync(
        int id,
        CustomerRequestDto request)
    {
        // Clean up input data and validate the date of birth.
        NormalizeRequest(request);

        ValidateDateOfBirth(request.DateOfBirth);

        // Find the customer to update.
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == id);

        // Return null if the customer does not exist.
        if (customer == null)
        {
            return null;
        }

        // Check for duplicate values while excluding the current customer.
        if (await _context.Customers.AnyAsync(x =>
            x.CustomerCode == request.CustomerCode &&
            x.Id != id))
        {
            throw new ArgumentException("Customer code already exists.");
        }

        if (await _context.Customers.AnyAsync(x =>
            x.PhoneNumber == request.PhoneNumber &&
            x.Id != id))
        {
            throw new ArgumentException("Phone number already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _context.Customers.AnyAsync(x =>
                x.Email == request.Email &&
                x.Id != id))
        {
            throw new ArgumentException("Email already exists.");
        }

        // Update the customer's information.
        customer.CustomerCode = request.CustomerCode;
        customer.FullName = request.FullName;
        customer.Email = request.Email;
        customer.PhoneNumber = request.PhoneNumber;
        customer.DateOfBirth = request.DateOfBirth;
        customer.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    // Delete a customer by ID.
    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == id);

        // Return false if the customer does not exist.
        if (customer == null)
        {
            return false;
        }

        // Remove the customer from the database.
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return true;
    }

    // Normalize user input before processing it.
    private static void NormalizeRequest(CustomerRequestDto request)
    {
        request.CustomerCode = request.CustomerCode.Trim();
        request.FullName = request.FullName.Trim();
        request.PhoneNumber = request.PhoneNumber.Trim();

        // Trim and lowercase email, or set it to null if empty.
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            request.Email = request.Email.Trim().ToLowerInvariant();
        }
        else
        {
            request.Email = null;
        }
    }

    // Validate that the date of birth is within a valid range.
    private static void ValidateDateOfBirth(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
        {
            return;
        }

        var dob = dateOfBirth.Value.Date;

        // Date of birth cannot be in the future.
        if (dob > DateTime.Today)
        {
            throw new ArgumentException(
                "Date of birth cannot be in the future.");
        }

        // Reject unrealistically old dates.
        if (dob < DateTime.Today.AddYears(-120))
        {
            throw new ArgumentException(
                "Date of birth is not valid.");
        }
    }

    // Convert a Customer entity into a CustomerDto.
    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            CustomerCode = customer.CustomerCode,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DateOfBirth = customer.DateOfBirth,
            IsActive = customer.IsActive
        };
    }
}