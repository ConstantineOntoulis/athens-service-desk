using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.DTOs.Auth;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Application.Services;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;
using System.Security.Authentication;

namespace AthensServiceDesk.Tests.Services
{
    public sealed class AuthServiceTests
    {
        private readonly FakeUserRepository _userRepository = new();
        private readonly FakePasswordService _passwordService = new();
        private readonly FakeJwtTokenService _jwtTokenService = new();
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _service = new AuthService(
                _userRepository,
                _passwordService,
                _jwtTokenService);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTokenAndUser_WhenCredentialsAreValid()
        {
            //Arange
            _userRepository.UserResult = CreateUser();
            _passwordService.VerificationResult = true;

            var request = new LoginRequest
            {
                Email = "Citizen@AthensDesk.Local",
                Password = "CorrectPassword!"
            };

            //Act
            LoginResponse result = await _service.LoginAsync(request);

            //Assert
            Assert.Equal(
                "CITIZEN@ATHENSDESK.LOCAL",
                _userRepository.LastNormalizedEmail);

            Assert.Equal(
                "test-access-token",
                result.AccessToken);

            Assert.Equal("Bearer", result.TokenType);
            Assert.Equal(1, result.User.Id);
            Assert.Equal(
                "citizen@athensdesk.local",
                result.User.Email);
            Assert.Equal("Citizen", result.User.Role);
            Assert.Equal(1, _jwtTokenService.CreateCallCount);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowInvalidCredentials_WhenUserDoesNotExist()
        {
            //Arrange
            _userRepository.UserResult = null;

            var request = new LoginRequest
            {
                Email = "missing@example.com",
                Password = "WrongPassword!"
            };

            //Act and assert
            await Assert.ThrowsAsync<
                InvalidCredentialException>(
                    () => _service.LoginAsync(request));

            Assert.Equal(0, _jwtTokenService.CreateCallCount);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowInvalidCredentials_WhenPasswordIsWrong()
        {
            //Arrange
            _userRepository.UserResult = CreateUser();
            _passwordService.VerificationResult = false;

            var request = new LoginRequest
            {
                Email = "citizen@athensdesk.local",
                Password = "WrongPassword!"
            };

            //Act and assert
            await Assert.ThrowsAsync<
                InvalidCredentialException>(
                () => _service.LoginAsync(request));

            Assert.Equal(0, _jwtTokenService.CreateCallCount);
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
            public AppUser? UserResult { get; set; }
            public string? LastNormalizedEmail
            {
                get;
                private set;
            }

            public Task<AppUser?>
                GetByNormalizedEmailAsync(
                    string normalizedEmail,
                    CancellationToken cancellationToken = default)
            {
                LastNormalizedEmail = normalizedEmail;

                return Task.FromResult(UserResult);
            }

            public Task<AppUser?> GetByIdAsync(
                int id,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(UserResult);
            }
        }

        private sealed class FakePasswordService : IPasswordService
        {
            public bool VerificationResult { get; set; }

            public string HashPassword(
                AppUser user,
                string password)
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
    }
}
