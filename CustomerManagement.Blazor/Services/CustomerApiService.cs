using CustomerManagement.Blazor.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CustomerManagement.Blazor.Services;

public class CustomerApiService
{
    // HTTP client used to communicate with the Customer API.
    private readonly HttpClient _httpClient;

    // Provides the JWT token of the logged-in user.
    private readonly AuthService _authService;

    // Create the API client and get the authentication service.
    public CustomerApiService(
        IHttpClientFactory httpClientFactory,
        AuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("Api");
        _authService = authService;
    }

    // Add the JWT token to the Authorization header.
    private void AddAuthorizationHeader()
    {
        // Clear the previous authorization header.
        _httpClient.DefaultRequestHeaders.Authorization = null;

        // Add the JWT token when the user is logged in.
        if (!string.IsNullOrEmpty(_authService.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _authService.Token);
        }
    }

    // Get all customers, optionally filtered by a search keyword.
    public async Task<List<CustomerDto>> GetCustomersAsync(
        string? search = null)
    {
        AddAuthorizationHeader();

        var url = "api/Customers";

        // Add the search keyword to the query string.
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search)}";
        }

        // Send the request and handle connection errors.
        var response = await SendAsync(
            () => _httpClient.GetAsync(url));

        // Throw a user-friendly error when the API request fails.
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }

        // Convert the API response into a list of customers.
        return await response.Content
                   .ReadFromJsonAsync<List<CustomerDto>>()
               ?? new List<CustomerDto>();
    }

    // Get a customer by ID.
    public async Task<CustomerDto?> GetCustomerAsync(int id)
    {
        AddAuthorizationHeader();

        var response = await SendAsync(
            () => _httpClient.GetAsync($"api/Customers/{id}"));

        // Return null if the customer does not exist.
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        // Handle other API errors.
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }

        // Convert the API response into a customer object.
        return await response.Content
            .ReadFromJsonAsync<CustomerDto>();
    }

    // Create a new customer.
    public async Task<CustomerDto> CreateCustomerAsync(object request)
    {
        AddAuthorizationHeader();

        var response = await SendAsync(
            () => _httpClient.PostAsJsonAsync(
                "api/Customers",
                request));

        // Handle API errors such as duplicate data.
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }

        // Convert the API response into the created customer.
        return await response.Content
                   .ReadFromJsonAsync<CustomerDto>()
               ?? throw new Exception(
                   "Customer was created, but the response was invalid.");
    }

    // Update an existing customer by ID.
    public async Task UpdateCustomerAsync(
        int id,
        object request)
    {
        AddAuthorizationHeader();

        var response = await SendAsync(
            () => _httpClient.PutAsJsonAsync(
                $"api/Customers/{id}",
                request));

        // Handle API errors.
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }
    }

    // Delete a customer by ID.
    public async Task DeleteCustomerAsync(int id)
    {
        AddAuthorizationHeader();

        var response = await SendAsync(
            () => _httpClient.DeleteAsync(
                $"api/Customers/{id}"));

        // Handle API errors.
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }
    }

    // Send an HTTP request and convert connection errors into a readable message.
    private async Task<HttpResponseMessage> SendAsync(
        Func<Task<HttpResponseMessage>> request)
    {
        try
        {
            return await request();
        }
        catch (HttpRequestException)
        {
            throw new Exception(
                "Unable to connect to the API. Please make sure the API is running.");
        }
    }

    // Read API error responses and convert them into readable exceptions.
    private static async Task ThrowApiError(
        HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var json = JsonDocument.Parse(content);
                var root = json.RootElement;

                // Read a custom error message returned by the backend.
                if (root.TryGetProperty(
                        "message",
                        out var message))
                {
                    var messageText = message.GetString();

                    if (!string.IsNullOrWhiteSpace(messageText))
                    {
                        throw new Exception(messageText);
                    }
                }

                // Read ASP.NET Core validation error messages.
                if (root.TryGetProperty(
                        "errors",
                        out var errors))
                {
                    foreach (var error in errors.EnumerateObject())
                    {
                        if (error.Value.ValueKind
                            != JsonValueKind.Array)
                        {
                            continue;
                        }

                        // Get the first validation message for the field.
                        var firstMessage =
                            error.Value.EnumerateArray()
                                .Select(x => x.GetString())
                                .FirstOrDefault(
                                    x => !string.IsNullOrWhiteSpace(x));

                        if (!string.IsNullOrWhiteSpace(firstMessage))
                        {
                            throw new Exception(firstMessage);
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore invalid JSON and use the HTTP status message.
            }
        }

        // Use a default message based on the HTTP status code.
        throw new Exception(
            GetStatusMessage(response.StatusCode));
    }

    // Convert HTTP status codes into user-friendly error messages.
    private static string GetStatusMessage(
        System.Net.HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest =>
                "The information entered is invalid. Please check your input.",

            System.Net.HttpStatusCode.Unauthorized =>
                "Your session has expired. Please log in again.",

            System.Net.HttpStatusCode.Forbidden =>
                "You do not have permission to perform this action.",

            System.Net.HttpStatusCode.NotFound =>
                "The requested customer was not found.",

            System.Net.HttpStatusCode.Conflict =>
                "This information already exists.",

            _ when (int)statusCode >= 500 =>
                "A server error occurred. Please try again later.",

            _ =>
                "The request could not be completed. Please try again."
        };
    }
}