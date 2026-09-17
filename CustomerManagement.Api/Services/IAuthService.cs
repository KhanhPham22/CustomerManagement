using CustomerManagement.Api.DTOs;

namespace CustomerManagement.Api.Services;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginRequestDto request);
}