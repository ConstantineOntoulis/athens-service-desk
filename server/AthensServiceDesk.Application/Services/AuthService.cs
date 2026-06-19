using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.DTOs.Auth;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Application.Interfaces.Services;
using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string normalizedEmail =
            request.Email
                .Trim()
                .ToUpperInvariant();

        AppUser? user =
            await _userRepository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        bool passwordIsValid =
            _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (!passwordIsValid)
        {
            throw new InvalidCredentialsException();
        }

        AccessTokenResult token =
            _jwtTokenService.CreateAccessToken(user);

        return new LoginResponse
        {
            AccessToken = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = new AuthenticatedUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString()
            }
        };
    }
}
