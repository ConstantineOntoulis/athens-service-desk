using System.Net;
using System.Net.Http.Json;
using AthensServiceDesk.Application.DTOs.Auth;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Domain.Enums;
using AthensServiceDesk.Infrastructure.Persistence;
using AthensServiceDesk.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AthensServiceDesk.IntegrationTests;

public sealed class ServiceRequestAssignmentTests
    : IDisposable
{
    private readonly CustomWebApplicationFactory
        _factory;

    private readonly HttpClient _client;

    public ServiceRequestAssignmentTests()
    {
        _factory =
            new CustomWebApplicationFactory();

        _client =
            _factory.CreateClient();
    }

    [Fact]
    public async Task Assign_ShouldAssignStaffAndCreateHistory_ForManager()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        int staffUserId =
            await GetUserIdAsync(
                AuthenticationTestHelper.StaffEmail);

        LoginResponse manager =
            await AuthenticationTestHelper
                .AuthenticateAsManagerAsync(_client);

        var assignment =
            new AssignServiceRequestRequest
            {
                StaffUserId = staffUserId,

                Note =
                    "Assigned to the infrastructure staff member."
            };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/assign",
                assignment);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        ServiceRequestDetailsResponse body =
            await response.Content
                .ReadFromJsonAsync<
                    ServiceRequestDetailsResponse>()
            ?? throw new InvalidOperationException(
                "The assignment response was empty.");

        Assert.Equal("Assigned", body.Status);

        Assert.Equal(
            staffUserId,
            body.AssignedToUserId);

        Assert.NotNull(body.AssignedAt);

        RequestStatusHistoryResponse history =
            Assert.Single(body.StatusHistory);

        Assert.Equal(
            "Submitted",
            history.PreviousStatus);

        Assert.Equal(
            "Assigned",
            history.NewStatus);

        Assert.Equal(
            manager.User.Id,
            history.ChangedByUserId);

        Assert.Equal(
            "Assigned to the infrastructure staff member.",
            history.Note);

        await AuthenticationTestHelper
            .AuthenticateAsStaffAsync(_client);

        HttpResponseMessage staffResponse =
            await _client.GetAsync(
                $"/api/service-requests/{created.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            staffResponse.StatusCode);
    }

    [Fact]
    public async Task Assign_ShouldReturnForbidden_ForCitizen()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        int staffUserId =
            await GetUserIdAsync(
                AuthenticationTestHelper.StaffEmail);

        var assignment =
            new AssignServiceRequestRequest
            {
                StaffUserId = staffUserId
            };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/assign",
                assignment);

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
    public async Task Assign_ShouldReturnConflict_WhenTargetUserIsNotStaff()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        int managerUserId =
            await GetUserIdAsync(
                AuthenticationTestHelper.ManagerEmail);

        await AuthenticationTestHelper
            .AuthenticateAsManagerAsync(_client);

        var assignment =
            new AssignServiceRequestRequest
            {
                StaffUserId = managerUserId
            };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/assign",
                assignment);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task Assign_ShouldReturnConflict_WhenRequestStatusCannotBeAssigned()
    {
        await AuthenticationTestHelper
            .AuthenticateAsCitizenAsync(_client);

        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        await SetStatusAsync(
            created.Id,
            ServiceRequestStatus.Closed);

        int staffUserId =
            await GetUserIdAsync(
                AuthenticationTestHelper.StaffEmail);

        await AuthenticationTestHelper
            .AuthenticateAsManagerAsync(_client);

        var assignment =
            new AssignServiceRequestRequest
            {
                StaffUserId = staffUserId
            };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/service-requests/{created.Id}/assign",
                assignment);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    private async Task<ServiceRequestDetailsResponse>
        CreateRequestAsync()
    {
        var request =
            new CreateServiceRequestRequest
            {
                Title =
                    "Broken streetlight for assignment",

                Description =
                    "This service request will be assigned to a staff member.",

                Location =
                    "Athens",

                DepartmentId = 2,

                ServiceCategoryId = 3,

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

        return await response.Content
            .ReadFromJsonAsync<
                ServiceRequestDetailsResponse>()
            ?? throw new InvalidOperationException(
                "The created response was empty.");
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
            .Where(user => user.Email == email)
            .Select(user => user.Id)
            .SingleAsync();
    }

    private async Task SetStatusAsync(
        int requestId,
        ServiceRequestStatus status)
    {
        using IServiceScope scope =
            _factory.Services.CreateScope();

        AppDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var serviceRequest =
            await dbContext.ServiceRequests
                .SingleAsync(
                    request =>
                        request.Id == requestId);

        serviceRequest.Status = status;

        await dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}