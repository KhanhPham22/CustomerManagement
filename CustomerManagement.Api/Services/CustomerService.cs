using CustomerManagement.Api.Data;
using CustomerManagement.Api.DTOs;
using CustomerManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetCustomersAsync(string? search)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.PhoneNumber.Contains(search));
        }

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

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
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

    public async Task<CustomerDto> CreateCustomerAsync(CustomerRequestDto request)
    {
        NormalizeRequest(request);

        ValidateDateOfBirth(request.DateOfBirth);

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

        var customer = new Customer
        {
            CustomerCode = request.CustomerCode,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            IsActive = request.IsActive
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(
        int id,
        CustomerRequestDto request)
    {
        NormalizeRequest(request);

        ValidateDateOfBirth(request.DateOfBirth);

        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (customer == null)
        {
            return null;
        }

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

        customer.CustomerCode = request.CustomerCode;
        customer.FullName = request.FullName;
        customer.Email = request.Email;
        customer.PhoneNumber = request.PhoneNumber;
        customer.DateOfBirth = request.DateOfBirth;
        customer.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (customer == null)
        {
            return false;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return true;
    }

    private static void NormalizeRequest(CustomerRequestDto request)
    {
        request.CustomerCode = request.CustomerCode.Trim();
        request.FullName = request.FullName.Trim();
        request.PhoneNumber = request.PhoneNumber.Trim();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            request.Email = request.Email.Trim().ToLowerInvariant();
        }
        else
        {
            request.Email = null;
        }
    }

    private static void ValidateDateOfBirth(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
        {
            return;
        }

        var dob = dateOfBirth.Value.Date;

        if (dob > DateTime.Today)
        {
            throw new ArgumentException(
                "Date of birth cannot be in the future.");
        }

        if (dob < DateTime.Today.AddYears(-120))
        {
            throw new ArgumentException(
                "Date of birth is not valid.");
        }
    }

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