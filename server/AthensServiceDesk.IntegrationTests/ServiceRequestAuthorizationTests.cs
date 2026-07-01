using System.Net;
using System.Net.Http.Json;
using AthensServiceDesk.Application.DTOs.Common;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Domain.Enums;
using AthensServiceDesk.Infrastructure.Persistence;
using AthensServiceDesk.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AthensServiceDesk.IntegrationTests;

public sealed class ServiceRequestAuthorizationTests
    : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ServiceRequestAuthorizationTests()
    {
        _factory =
            new CustomWebApplicationFactory();

        _client =
            _factory.CreateClient();
    }

    [Fact]
    public async Task List_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        HttpResponseMessage response =
            await _client.GetAsync(
                "/api/service-requests");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);

        ProblemDetails problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>()
            ?? throw new InvalidOperationException(
                "The problem response was empty.");

        Assert.Equal(
            "Authentication required",
            problem.Title);
    }

    [Fact]
    public async Task Create_ShouldReturnForbidden_ForStaff()
    {
        await AuthenticationTestHelper
            .AuthenticateAsStaffAsync(_client);

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/service-requests",
                CreateValidRequest(
                    "Staff request"));

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
    public async Task CitizenList_ShouldReturnOnlyOwnedRequests()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse ownRequest =
            await CreateRequestAsync(
                "Citizen-owned request");

        ServiceRequestDetailsResponse otherRequest =
            await CreateRequestAsync(
                "Other-user request");

        await ChangeOwnerAsync(
            otherRequest.Id,
            AuthenticationTestHelper.ManagerEmail);

        HttpResponseMessage response =
            await _client.GetAsync(
                "/api/service-requests");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        PagedResponse<ServiceRequestResponse> body =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResponse<ServiceRequestResponse>>()
            ?? throw new InvalidOperationException(
                "The response body was empty.");

        ServiceRequestResponse visibleRequest =
            Assert.Single(body.Items);

        Assert.Equal(
            ownRequest.Id,
            visibleRequest.Id);
    }

    [Fact]
    public async Task StaffList_ShouldReturnOnlyAssignedRequests()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse assigned =
            await CreateRequestAsync(
                "Assigned request");

        await CreateRequestAsync(
            "Unassigned request");

        await AssignRequestAsync(
            assigned.Id,
            AuthenticationTestHelper.StaffEmail);

        await AuthenticationTestHelper
            .AuthenticateAsStaffAsync(_client);

        HttpResponseMessage response =
            await _client.GetAsync(
                "/api/service-requests");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        PagedResponse<ServiceRequestResponse> body =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResponse<ServiceRequestResponse>>()
            ?? throw new InvalidOperationException(
                "The response body was empty.");

        ServiceRequestResponse visibleRequest =
            Assert.Single(body.Items);

        Assert.Equal(
            assigned.Id,
            visibleRequest.Id);
    }

    [Fact]
    public async Task ManagerList_ShouldReturnAllRequests()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        await CreateRequestAsync(
            "First request");

        await CreateRequestAsync(
            "Second request");

        await AuthenticationTestHelper
            .AuthenticateAsManagerAsync(_client);

        HttpResponseMessage response =
            await _client.GetAsync(
                "/api/service-requests");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        PagedResponse<ServiceRequestResponse> body =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResponse<ServiceRequestResponse>>()
            ?? throw new InvalidOperationException(
                "The response body was empty.");

        Assert.Equal(
            2,
            body.TotalCount);

        Assert.Equal(
            2,
            body.Items.Count);
    }

    [Fact]
    public async Task GetById_ShouldReturnForbidden_WhenCitizenDoesNotOwnRequest()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse request =
            await CreateRequestAsync(
                "Another user's request");

        await ChangeOwnerAsync(
            request.Id,
            AuthenticationTestHelper.ManagerEmail);

        HttpResponseMessage response =
            await _client.GetAsync(
                $"/api/service-requests/{request.Id}");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnForbidden_WhenCitizenDoesNotOwnRequest()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse request =
            await CreateRequestAsync(
                "Another user's request");

        await ChangeOwnerAsync(
            request.Id,
            AuthenticationTestHelper.ManagerEmail);

        var update =
            new UpdateServiceRequestRequest
            {
                Title =
                    "Attempted unauthorized update",

                Description =
                    "The current citizen does not own this request.",

                Location = "Athens",

                DepartmentId = 2,

                ServiceCategoryId = 3,

                Priority =
                    ServicePriority.High
            };

        HttpResponseMessage response =
            await _client.PutAsJsonAsync(
                $"/api/service-requests/{request.Id}",
                update);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private async Task<
        ServiceRequestDetailsResponse> CreateRequestAsync(
        string title)
    {
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/service-requests",
                CreateValidRequest(title));

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        return await response.Content
            .ReadFromJsonAsync<
                ServiceRequestDetailsResponse>()
            ?? throw new InvalidOperationException(
                "The created response was empty.");
    }

    private async Task ChangeOwnerAsync(
        int requestId,
        string ownerEmail)
    {
        using IServiceScope scope =
            _factory.Services.CreateScope();

        AppDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        int ownerId =
            await dbContext.Users
                .Where(
                    user =>
                        user.Email == ownerEmail)
                .Select(user => user.Id)
                .SingleAsync();

        var request =
            await dbContext.ServiceRequests
                .SingleAsync(
                    item =>
                        item.Id == requestId);

        request.CreatedByUserId =
            ownerId;

        await dbContext.SaveChangesAsync();
    }

    private async Task AssignRequestAsync(
        int requestId,
        string staffEmail)
    {
        using IServiceScope scope =
            _factory.Services.CreateScope();

        AppDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        int staffId =
            await dbContext.Users
                .Where(
                    user =>
                        user.Email == staffEmail)
                .Select(user => user.Id)
                .SingleAsync();

        var request =
            await dbContext.ServiceRequests
                .SingleAsync(
                    item =>
                        item.Id == requestId);

        request.AssignedToUserId =
            staffId;

        request.AssignedAt =
            DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
    }

    private static CreateServiceRequestRequest
        CreateValidRequest(string title)
    {
        return new CreateServiceRequestRequest
        {
            Title = title,

            Description =
                "This is a sufficiently detailed service request description.",

            Location = "Athens",

            DepartmentId = 2,

            ServiceCategoryId = 3,

            Priority =
                ServicePriority.Medium
        };
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}