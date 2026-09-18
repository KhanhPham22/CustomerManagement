namespace CustomerManagement.Blazor.Services;

public class AuthService
{
    // Stores the JWT token of the logged-in user.
    public string? Token { get; private set; }

    // Determines whether the user is currently logged in.
    public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    // Store the JWT token after a successful login.
    public void SetToken(string token)
    {
        Token = token;
    }

    // Clear the token and log the user out.
    public void Logout()
    {
        Token = null;
    }
}