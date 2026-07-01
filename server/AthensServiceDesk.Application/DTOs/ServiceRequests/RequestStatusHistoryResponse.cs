namespace AthensServiceDesk.Application.DTOs.ServiceRequests;

public sealed class RequestStatusHistoryResponse
{
    public int Id { get; set; }

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public int ChangedByUserId { get; set; }

    public string ChangedByUserName { get; set; } = string.Empty;

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}