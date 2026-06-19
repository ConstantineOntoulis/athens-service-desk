namespace AthensServiceDesk.Application.DTOs.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTimeOffset ExpiresAt { get; set; }
    public AuthenticatedUserResponse User { get; set; } = new();
}
