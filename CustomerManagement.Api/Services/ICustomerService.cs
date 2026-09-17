using CustomerManagement.Api.DTOs;

namespace CustomerManagement.Api.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetCustomersAsync(string? search);
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CustomerRequestDto request);
    Task<CustomerDto?> UpdateCustomerAsync(int id, CustomerRequestDto request);
    Task<bool> DeleteCustomerAsync(int id);
}