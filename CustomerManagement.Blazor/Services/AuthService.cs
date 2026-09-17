namespace CustomerManagement.Blazor.Services;

public class AuthService
{
    public string? Token { get; private set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    public void SetToken(string token)
    {
        Token = token;
    }

    public void Logout()
    {
        Token = null;
    }
}