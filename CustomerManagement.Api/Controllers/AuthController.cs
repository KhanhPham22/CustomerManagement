using CustomerManagement.Api.DTOs;
using CustomerManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagement.Api.Controllers;

// Handles authentication-related API requests such as login
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Provides authentication logic for the controller
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Allows users to log in without authentication
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto request)
    {
        // Validates the login information and generates a JWT token
        var token = await _authService.LoginAsync(request);

        // Return 401 if the login information is invalid
        if (token == null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        // Return the JWT token when login is successful
        return Ok(new LoginResponseDto
        {
            Token = token
        });
    }
}