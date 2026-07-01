using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Application.Common.Models;

public sealed record ServiceRequestAccessScope(int UserId, UserRole Role);
