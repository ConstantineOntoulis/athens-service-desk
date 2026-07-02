using System.Net;
using System.Net.Http.Json;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Domain.Enums;
using AthensServiceDesk.Infrastructure.Persistence;
using AthensServiceDesk.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AthensServiceDesk.IntegrationTests;

public sealed class ServiceRequestWorkProgressionTests
    : IDisposable
{
    private readonly CustomWebApplicationFactory
        _factory;

    private readonly HttpClient
        _client;

    public ServiceRequestWorkProgressionTests()
    {
        _factory =
            new CustomWebApplicationFactory();

        _client =
            _factory.CreateClient();
    }

    [Fact]
    public async Task Staff_ShouldStartAndResolveAssignedRequest()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        int staffUserId =
            await GetUserIdAsync(
                AuthenticationTestHelper.StaffEmail);

        await AuthenticationTestHelper
            .AuthenticateAsManagerAsync(_client);

        await AssignRequestAsync(
            created.Id,
            staffUserId);

        await AuthenticationTestHelper
            .AuthenticateAsStaffAsync(_client);

        var startRequest =
            new StartServiceRequestRequest
            {
                Note =
                    "The staff member started the inspection."
            };

        HttpResponseMessage startResponse =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/start",
                startRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            startResponse.StatusCode);

        ServiceRequestDetailsResponse started =
            await ReadDetailsAsync(
                startResponse);

        Assert.Equal(
            "InProgress",
            started.Status);

        Assert.Contains(
            started.StatusHistory,
            history =>
                history.PreviousStatus == "Assigned"
                && history.NewStatus == "InProgress");

        var resolveRequest =
            new ResolveServiceRequestRequest
            {
                ResolutionNote =
                    "The faulty streetlight lamp was replaced and tested successfully."
            };

        HttpResponseMessage resolveResponse =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/resolve",
                resolveRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            resolveResponse.StatusCode);

        ServiceRequestDetailsResponse resolved =
            await ReadDetailsAsync(
                resolveResponse);

        Assert.Equal(
            "Resolved",
            resolved.Status);

        Assert.NotNull(
            resolved.ResolvedAt);

        Assert.Contains(
            resolved.StatusHistory,
            history =>
                history.PreviousStatus == "InProgress"
                && history.NewStatus == "Resolved"
                && history.Note ==
                    "The faulty streetlight lamp was replaced and tested successfully.");
    }

    [Fact]
    public async Task Start_ShouldReturnForbidden_WhenRequestIsNotAssignedToStaff()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        await AuthenticationTestHelper
            .AuthenticateAsStaffAsync(_client);

        var request =
            new StartServiceRequestRequest();

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/start",
                request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        ProblemDetails problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>()
            ?? throw new InvalidOperationException(
                "The problem response was empty.");

        Assert.Equal(
            "Access forbidden",
            problem.Title);
    }

    [Fact]
    public async Task Start_ShouldReturnForbidden_ForCitizen()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        var request =
            new StartServiceRequestRequest();

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/start",
                request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Resolve_ShouldReturnConflict_WhenRequestHasNotBeenStarted()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        int staffUserId =
            await GetUserIdAsync(
                AuthenticationTestHelper.StaffEmail);

        await AuthenticationTestHelper
            .AuthenticateAsManagerAsync(_client);

        await AssignRequestAsync(
            created.Id,
            staffUserId);

        await AuthenticationTestHelper
            .AuthenticateAsStaffAsync(_client);

        var request =
            new ResolveServiceRequestRequest
            {
                ResolutionNote =
                    "The reported issue has been resolved successfully."
            };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/resolve",
                request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        ProblemDetails problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>()
            ?? throw new InvalidOperationException(
                "The problem response was empty.");

        Assert.Equal(
            "Business rule violation",
            problem.Title);
    }

    private async Task<ServiceRequestDetailsResponse>
        CreateRequestAsync()
    {
        var request =
            new CreateServiceRequestRequest
            {
                Title =
                    "Streetlight issue for workflow progression",

                Description =
                    "This service request is used to test the staff workflow progression.",

                Location =
                    "Athens",

                DepartmentId =
                    2,

                ServiceCategoryId =
                    3,

                Priority =
                    ServicePriority.Medium
            };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/service-requests",
                request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        return await ReadDetailsAsync(
            response);
    }

    private async Task AssignRequestAsync(
        int serviceRequestId,
        int staffUserId)
    {
        var request =
            new AssignServiceRequestRequest
            {
                StaffUserId =
                    staffUserId,

                Note =
                    "Assigned for workflow progression testing."
            };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{serviceRequestId}/assign",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    private async Task<int> GetUserIdAsync(
        string email)
    {
        using IServiceScope scope =
            _factory.Services.CreateScope();

        AppDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        return await dbContext.Users
            .Where(
                user =>
                    user.Email == email)
            .Select(
                user =>
                    user.Id)
            .SingleAsync();
    }

    private static async Task<
        ServiceRequestDetailsResponse> ReadDetailsAsync(
        HttpResponseMessage response)
    {
        return await response.Content
            .ReadFromJsonAsync<
                ServiceRequestDetailsResponse>()
            ?? throw new InvalidOperationException(
                "The service request response body was empty.");
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}