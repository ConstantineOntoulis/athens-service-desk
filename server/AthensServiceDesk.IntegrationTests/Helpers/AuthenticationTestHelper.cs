using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AthensServiceDesk.Application.DTOs.Auth;

namespace AthensServiceDesk.IntegrationTests.Helpers;

internal static class AuthenticationTestHelper
{
    public const string CitizenEmail =
        "citizen@athensdesk.local";

    public const string StaffEmail =
        "staff@athensdesk.local";

    public const string ManagerEmail =
        "manager@athensdesk.local";

    public const string AdminEmail =
        "admin@athensdesk.local";

    public static Task<LoginResponse>
        AuthenticateAsCitizenAsync(
            HttpClient client)
    {
        return AuthenticateAsync(
            client,
            CitizenEmail);
    }

    public static Task<LoginResponse>
        AuthenticateAsStaffAsync(
            HttpClient client)
    {
        return AuthenticateAsync(
            client,
            StaffEmail);
    }

    public static Task<LoginResponse>
        AuthenticateAsManagerAsync(
            HttpClient client)
    {
        return AuthenticateAsync(
            client,
            ManagerEmail);
    }

    public static Task<LoginResponse>
        AuthenticateAsAdminAsync(
            HttpClient client)
    {
        return AuthenticateAsync(
            client,
            AdminEmail);
    }

    private static async Task<LoginResponse>
        AuthenticateAsync(
            HttpClient client,
            string email)
    {
        client.DefaultRequestHeaders.Authorization =
            null;

        var request =
            new LoginRequest
            {
                Email = email,

                Password =
                    CustomWebApplicationFactory
                        .DemoPassword
            };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(
                $"Authentication failed for '{email}' " +
                $"with status {(int)response.StatusCode}.");
        }

        LoginResponse login =
            await response.Content
                .ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException(
                "The login response body was empty.");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        return login;
    }
}