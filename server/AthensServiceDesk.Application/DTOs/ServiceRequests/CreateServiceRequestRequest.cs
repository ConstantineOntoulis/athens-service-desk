using System.ComponentModel.DataAnnotations;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Application.DTOs.ServiceRequests;

public class CreateServiceRequestRequest
{
    [Required]
    [StringLength(150, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(300, MinimumLength =3)]
    public string Location {  get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServiceCategoryId {  get; set; }
    [EnumDataType(typeof(ServicePriority))]
    public ServicePriority Priority { get; set; } = ServicePriority.Medium;
}