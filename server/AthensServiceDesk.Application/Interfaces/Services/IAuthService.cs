using AthensServiceDesk.Application.DTOs.Auth;

namespace AthensServiceDesk.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUserResponse> GetCurrentUserAsync(
        CancellationToken cancellationToken = default);
}
