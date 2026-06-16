using System.Net;
using System.Net.Http.Json;
using AthensServiceDesk.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;

namespace AthensServiceDesk.IntegrationTests;

public sealed class DepartmentEndpointsTests
    : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DepartmentEndpointsTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetDepartments_ShouldReturnActiveDepartments()
    {
        // Act
        HttpResponseMessage response =
            await _client.GetAsync("/api/departments");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        List<DepartmentResponse> body =
            await response.Content
                .ReadFromJsonAsync<List<DepartmentResponse>>()
            ?? throw new InvalidOperationException(
                "The response body was empty.");

        Assert.Equal(4, body.Count);

        Assert.Equal(
            [
                "Citizen Services",
                "Digital Services",
                "Infrastructure",
                "Maintenance"
            ],
            body.Select(department => department.Name));
    }

    [Fact]
    public async Task GetServiceCategories_ShouldReturnCategoriesForDepartment()
    {
        // Act
        HttpResponseMessage response =
            await _client.GetAsync(
                "/api/departments/2/service-categories");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        List<ServiceCategoryResponse> body =
            await response.Content
                .ReadFromJsonAsync<
                    List<ServiceCategoryResponse>>()
            ?? throw new InvalidOperationException(
                "The response body was empty.");

        Assert.Equal(2, body.Count);

        Assert.All(
            body,
            category => Assert.Equal(
                2,
                category.DepartmentId));

        Assert.Equal(
            [
                "Road Damage",
                "Streetlight Issue"
            ],
            body.Select(category => category.Name));
    }

    [Fact]
    public async Task GetServiceCategories_ShouldReturnNotFound_WhenDepartmentDoesNotExist()
    {
        // Act
        HttpResponseMessage response =
            await _client.GetAsync(
                "/api/departments/999999/service-categories");

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
        Assert.Equal(
            "Resource not found",
            problem.Title);

        Assert.NotNull(problem.Detail);
        Assert.Contains("999999", problem.Detail);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}