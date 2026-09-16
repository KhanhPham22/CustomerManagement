using CustomerManagement.Api.DTOs;

namespace CustomerManagement.Api.Services;
//interface to crud customer feature
public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync(string? search);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CustomerRequestDto request);
    Task<bool> UpdateAsync(int id, CustomerRequestDto request);
    Task<bool> DeleteAsync(int id);
}