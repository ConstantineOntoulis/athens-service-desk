using System.Net;
using System.Net.Http.Json;
using AthensServiceDesk.Application.DTOs.Common;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AthensServiceDesk.IntegrationTests;

public sealed class ServiceRequestsEndpointTests
    : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ServiceRequestsEndpointTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk()
    {
        // Act
        HttpResponseMessage response =
            await _client.GetAsync("/api/service-requests");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResponse<ServiceRequestResponse> body =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResponse<ServiceRequestResponse>>()
            ?? throw new InvalidOperationException(
                "The response body was empty.");

        Assert.Equal(1, body.Page);
        Assert.Equal(10, body.PageSize);
        Assert.Equal(0, body.TotalCount);
        Assert.Empty(body.Items);
    }

    [Fact]
    public async Task Post_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        CreateServiceRequestRequest request =
            CreateValidRequest();

        // Act
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/service-requests",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        ServiceRequestDetailsResponse body =
            await ReadDetailsResponseAsync(response);

        Assert.True(body.Id > 0);
        Assert.Equal(request.Title, body.Title);
        Assert.Equal("Submitted", body.Status);
        Assert.Equal("High", body.Priority);
        Assert.Equal(2, body.DepartmentId);
        Assert.Equal(3, body.ServiceCategoryId);

        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRequestExists()
    {
        // Arrange
        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        // Act
        HttpResponseMessage response =
            await _client.GetAsync(
                $"/api/service-requests/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ServiceRequestDetailsResponse body =
            await ReadDetailsResponseAsync(response);

        Assert.Equal(created.Id, body.Id);
        Assert.Equal(created.Title, body.Title);
        Assert.Equal("Infrastructure", body.DepartmentName);
        Assert.Equal(
            "Streetlight Issue",
            body.ServiceCategoryName);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        // Act
        HttpResponseMessage response =
            await _client.GetAsync(
                "/api/service-requests/999999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        ProblemDetails problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>()
            ?? throw new InvalidOperationException(
                "The problem response body was empty.");

        Assert.Equal(404, problem.Status);
        Assert.Equal("Resource not found", problem.Title);
    }

    [Fact]
    public async Task Post_ShouldReturnBadRequest_WhenDtoIsInvalid()
    {
        // Arrange
        var invalidRequest = new
        {
            title = "Bad",
            description = "Too short",
            location = "",
            departmentId = 0,
            serviceCategoryId = 0,
            priority = 2
        };

        // Act
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/service-requests",
                invalidRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldReturnConflict_WhenCategoryDoesNotBelongToDepartment()
    {
        // Arrange
        var request = new CreateServiceRequestRequest
        {
            Title = "Mismatched department and category",
            Description =
                "This request intentionally uses a category " +
                "that belongs to another department.",
            Location = "Athens",
            DepartmentId = 1,
            ServiceCategoryId = 3,
            Priority = ServicePriority.Medium
        };

        // Act
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/service-requests",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        ProblemDetails problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>()
            ?? throw new InvalidOperationException(
                "The problem response body was empty.");

        Assert.Equal(409, problem.Status);
        Assert.Equal(
            "Business rule violation",
            problem.Title);

        Assert.Contains(
            "does not belong",
            problem.Detail);
    }

    [Fact]
    public async Task Put_ShouldReturnOk_WhenRequestIsEditable()
    {
        // Arrange
        ServiceRequestDetailsResponse created =
            await CreateRequestAsync();

        var updateRequest =
            new UpdateServiceRequestRequest
            {
                Title =
                    "Updated broken streetlight report",
                Description =
                    "The streetlight remains broken and " +
                    "the pedestrian area is still dark.",
                Location =
                    "Central Syntagma Square, Athens",
                DepartmentId = 2,
                ServiceCategoryId = 3,
                Priority = ServicePriority.Urgent
            };

        // Act
        HttpResponseMessage response =
            await _client.PutAsJsonAsync(
                $"/api/service-requests/{created.Id}",
                updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ServiceRequestDetailsResponse body =
            await ReadDetailsResponseAsync(response);

        Assert.Equal(updateRequest.Title, body.Title);
        Assert.Equal(
            updateRequest.Description,
            body.Description);
        Assert.Equal(
            updateRequest.Location,
            body.Location);
        Assert.Equal("Urgent", body.Priority);
        Assert.NotNull(body.UpdatedAt);
    }

    private async Task<ServiceRequestDetailsResponse>
        CreateRequestAsync()
    {
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/service-requests",
                CreateValidRequest());

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        return await ReadDetailsResponseAsync(response);
    }

    private static CreateServiceRequestRequest
        CreateValidRequest()
    {
        return new CreateServiceRequestRequest
        {
            Title =
                "Broken streetlight near Syntagma Square",
            Description =
                "The streetlight has not worked for " +
                "several nights and the area is dark.",
            Location = "Syntagma Square, Athens",
            DepartmentId = 2,
            ServiceCategoryId = 3,
            Priority = ServicePriority.High
        };
    }

    private static async Task<
        ServiceRequestDetailsResponse>
        ReadDetailsResponseAsync(
            HttpResponseMessage response)
    {
        return await response.Content
            .ReadFromJsonAsync<
                ServiceRequestDetailsResponse>()
            ?? throw new InvalidOperationException(
                "The response body was empty.");
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}