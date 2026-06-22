using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.DTOs.Auth;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Application.Services;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Services;

public sealed class AuthServiceTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordService _passwordService = new();
    private readonly FakeJwtTokenService _jwtTokenService = new();
    private readonly FakeCurrentUserService _currentUserService = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _userRepository,
            _passwordService,
            _jwtTokenService,
            _currentUserService);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenAndUser_WhenCredentialsAreValid()
    {
        _userRepository.EmailLookupResult = CreateUser();
        _passwordService.VerificationResult = true;

        var request = new LoginRequest
        {
            Email = " Citizen@AthensDesk.Local ",
            Password = "CorrectPassword!"
        };

        LoginResponse result = await _service.LoginAsync(request);

        Assert.Equal(
            "CITIZEN@ATHENSDESK.LOCAL",
            _userRepository.LastNormalizedEmail);

        Assert.Equal("test-access-token", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(1, result.User.Id);
        Assert.Equal("citizen@athensdesk.local", result.User.Email);
        Assert.Equal("Citizen", result.User.Role);
        Assert.Equal(1, _jwtTokenService.CreateCallCount);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowInvalidCredentials_WhenUserDoesNotExist()
    {
        _userRepository.EmailLookupResult = null;

        var request = new LoginRequest
        {
            Email = "missing@example.com",
            Password = "WrongPassword!"
        };

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _service.LoginAsync(request));

        Assert.Equal(0, _jwtTokenService.CreateCallCount);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowInvalidCredentials_WhenPasswordIsWrong()
    {
        _userRepository.EmailLookupResult = CreateUser();
        _passwordService.VerificationResult = false;

        var request = new LoginRequest
        {
            Email = "citizen@athensdesk.local",
            Password = "WrongPassword!"
        };

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _service.LoginAsync(request));

        Assert.Equal(0, _jwtTokenService.CreateCallCount);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnCurrentActiveUser()
    {
        _currentUserService.IsAuthenticated = true;
        _currentUserService.UserId = 1;
        _userRepository.IdLookupResult = CreateUser();

        AuthenticatedUserResponse result =
            await _service.GetCurrentUserAsync();

        Assert.Equal(1, result.Id);
        Assert.Equal("citizen@athensdesk.local", result.Email);
        Assert.Equal("Demo", result.FirstName);
        Assert.Equal("Citizen", result.LastName);
        Assert.Equal("Citizen", result.Role);
        Assert.Equal(1, _userRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldThrow_WhenIdentityIsNotAuthenticated()
    {
        _currentUserService.IsAuthenticated = false;
        _currentUserService.UserId = null;

        await Assert.ThrowsAsync<UnauthenticatedException>(
            () => _service.GetCurrentUserAsync());

        Assert.Equal(0, _userRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldThrow_WhenUserNoLongerExists()
    {
        _currentUserService.IsAuthenticated = true;
        _currentUserService.UserId = 99;
        _userRepository.IdLookupResult = null;

        await Assert.ThrowsAsync<UnauthenticatedException>(
            () => _service.GetCurrentUserAsync());

        Assert.Equal(1, _userRepository.GetByIdCallCount);
    }

    private static AppUser CreateUser()
    {
        return new AppUser
        {
            Id = 1,
            Email = "citizen@athensdesk.local",
            NormalizedEmail = "CITIZEN@ATHENSDESK.LOCAL",
            PasswordHash = "stored-hash",
            FirstName = "Demo",
            LastName = "Citizen",
            Role = UserRole.Citizen,
            IsActive = true
        };
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public AppUser? EmailLookupResult { get; set; }

        public AppUser? IdLookupResult { get; set; }

        public string? LastNormalizedEmail { get; private set; }

        public int GetByIdCallCount { get; private set; }

        public Task<AppUser?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken = default)
        {
            LastNormalizedEmail = normalizedEmail;

            return Task.FromResult(EmailLookupResult);
        }

        public Task<AppUser?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;

            return Task.FromResult(IdLookupResult);
        }
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public bool VerificationResult { get; set; }

        public string HashPassword(AppUser user, string password)
        {
            return "generated-hash";
        }

        public bool VerifyPassword(
            AppUser user,
            string passwordHash,
            string providedPassword)
        {
            return VerificationResult;
        }
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public int CreateCallCount { get; private set; }

        public AccessTokenResult CreateAccessToken(AppUser user)
        {
            CreateCallCount++;

            return new AccessTokenResult(
                "test-access-token",
                new DateTimeOffset(
                    2026,
                    6,
                    18,
                    20,
                    0,
                    0,
                    TimeSpan.Zero));
        }
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated { get; set; }

        public int? UserId { get; set; }

        public string? Email { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public UserRole? Role { get; set; }
    }
}