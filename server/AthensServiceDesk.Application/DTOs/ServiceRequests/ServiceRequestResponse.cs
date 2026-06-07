namespace AthensServiceDesk.Application.DTOs.ServiceRequests;

public class ServiceRequestResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public string ServiceCategoryName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}