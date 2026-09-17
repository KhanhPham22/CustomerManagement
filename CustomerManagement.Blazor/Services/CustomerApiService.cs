using CustomerManagement.Blazor.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CustomerManagement.Blazor.Services;

public class CustomerApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public CustomerApiService(
        IHttpClientFactory httpClientFactory,
        AuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("Api");
        _authService = authService;
    }

    private void AddAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrEmpty(_authService.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _authService.Token);
        }
    }

    public async Task<List<CustomerDto>> GetCustomersAsync(
        string? search = null)
    {
        AddAuthorizationHeader();

        var url = "api/Customers";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search)}";
        }

        var response = await SendAsync(
            () => _httpClient.GetAsync(url));

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }

        return await response.Content
                   .ReadFromJsonAsync<List<CustomerDto>>()
               ?? new List<CustomerDto>();
    }

    public async Task<CustomerDto?> GetCustomerAsync(int id)
    {
        AddAuthorizationHeader();

        var response = await SendAsync(
            () => _httpClient.GetAsync($"api/Customers/{id}"));

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }

        return await response.Content
            .ReadFromJsonAsync<CustomerDto>();
    }

    public async Task<CustomerDto> CreateCustomerAsync(object request)
    {
        AddAuthorizationHeader();

        var response = await SendAsync(
            () => _httpClient.PostAsJsonAsync(
                "api/Customers",
                request));

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }

        return await response.Content
                   .ReadFromJsonAsync<CustomerDto>()
               ?? throw new Exception(
                   "Customer was created, but the response was invalid.");
    }

    public async Task UpdateCustomerAsync(
        int id,
        object request)
    {
        AddAuthorizationHeader();

        var response = await SendAsync(
            () => _httpClient.PutAsJsonAsync(
                $"api/Customers/{id}",
                request));

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }
    }

    public async Task DeleteCustomerAsync(int id)
    {
        AddAuthorizationHeader();

        var response = await SendAsync(
            () => _httpClient.DeleteAsync(
                $"api/Customers/{id}"));

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiError(response);
        }
    }

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

                // Backend custom error:
                // { "message": "Customer code already exists." }
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

                // ASP.NET Core validation error:
                // {
                //   "errors": {
                //      "PhoneNumber": [
                //          "Phone number must be..."
                //      ]
                //   }
                // }
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
                // Ignore invalid JSON and use status-based message.
            }
        }

        throw new Exception(
            GetStatusMessage(response.StatusCode));
    }

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