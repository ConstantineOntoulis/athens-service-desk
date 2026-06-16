namespace AthensServiceDesk.Application.DTOs.Lookups;

public sealed class ServiceCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
}
