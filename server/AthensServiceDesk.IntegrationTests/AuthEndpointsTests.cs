using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AthensServiceDesk.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AthensServiceDesk.IntegrationTests;

public sealed class AuthEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        LoginResponse body = await LoginAsCitizenAsync();

        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal(2, body.AccessToken.Count(character => character == '.'));
        Assert.Equal("Bearer", body.TokenType);
        Assert.Equal("citizen@athensdesk.local", body.User.Email);
        Assert.Equal("Citizen", body.User.Role);
        Assert.True(body.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        var request = new LoginRequest
        {
            Email = "citizen@athensdesk.local",
            Password = "IncorrectPassword!"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        ProblemDetails problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>()
            ?? throw new InvalidOperationException(
                "The problem response body was empty.");

        Assert.Equal(401, problem.Status);
        Assert.Equal("Authentication failed", problem.Title);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenDtoIsInvalid()
    {
        var invalidRequest = new
        {
            email = "not-an-email",
            password = "short"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            invalidRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturnCurrentUser_WhenBearerTokenIsValid()
    {
        LoginResponse login = await LoginAsCitizenAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthenticatedUserResponse body =
            await response.Content.ReadFromJsonAsync<AuthenticatedUserResponse>()
            ?? throw new InvalidOperationException(
                "The current-user response body was empty.");

        Assert.Equal(login.User.Id, body.Id);
        Assert.Equal(login.User.Email, body.Email);
        Assert.Equal("Demo", body.FirstName);
        Assert.Equal("Citizen", body.LastName);
        Assert.Equal("Citizen", body.Role);
    }

    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-valid-token");

        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<LoginResponse> LoginAsCitizenAsync()
    {
        var request = new LoginRequest
        {
            Email = "citizen@athensdesk.local",
            Password = CustomWebApplicationFactory.DemoPassword
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException(
                "The login response body was empty.");
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}