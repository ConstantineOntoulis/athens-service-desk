using AthensServiceDesk.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

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
        //Arrange
        var request = new LoginRequest
        {
            Email = "citizen@athensdesk.local",
            Password = CustomWebApplicationFactory.DemoPassword
        };

        //Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        LoginResponse body = await response.Content.ReadFromJsonAsync<LoginResponse>() ?? throw new InvalidOperationException("The login response body was empty.");

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
        //Arrange
        var request = new LoginRequest
        {
            Email = "citizen@athensdesk.local",
            Password = "IncorrectPassword!"
        };

        //Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login", request);

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        ProblemDetails problem = await response.Content.ReadFromJsonAsync<ProblemDetails>() ?? throw new InvalidOperationException("The problem response body was empty.");

        Assert.Equal(401, problem.Status);
        Assert.Equal("Authentication failed", problem.Title);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenDtoIsInvalid()
    {
        //Arrange
        var invalidRequest = new
        {
            email = "not-an-email",
            password = "short"
        };

        //Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login", invalidRequest);

        //Assert
        Assert.Equal(
            HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
