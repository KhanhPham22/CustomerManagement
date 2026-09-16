using CustomerManagement.Blazor.Models;
using System.Net.Http.Json;

namespace CustomerManagement.Blazor.Services;

public class CustomerApiService
{
    private readonly HttpClient _httpClient;

    public CustomerApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CustomerDto>> GetCustomersAsync(string? search = null)
    {
        var url = "api/Customers";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search)}";
        }

        return await _httpClient.GetFromJsonAsync<List<CustomerDto>>(url)
               ?? new List<CustomerDto>();
    }

    public async Task<CustomerDto?> GetCustomerAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<CustomerDto>(
            $"api/Customers/{id}");
    }

    public async Task<CustomerDto> CreateCustomerAsync(object request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Customers", request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerDto>()
               ?? throw new Exception("Failed to create customer.");
    }

    public async Task UpdateCustomerAsync(int id, object request)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/Customers/{id}", request);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/Customers/{id}");

        response.EnsureSuccessStatusCode();
    }
}