using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Domain.Enums;
using System.Globalization;
using System.Security.Claims;

namespace AthensServiceDesk.Api.Security;

public sealed class HttpCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int? UserId
    {
        get
        {
            string? value = GetClaimValue("sub");

            return int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int userId)
                    ? userId : null;
        }
    }

    public string? Email => GetClaimValue("email");
    public string? FirstName => GetClaimValue("given_name");
    public string? LastName => GetClaimValue("family_name");

    public UserRole? Role
    {
        get
        {
            string? value = GetClaimValue("role");

            return Enum.TryParse(
                value,
                ignoreCase: true,
                out UserRole role)
                    ? role : null;
        }
    }

    private string? GetClaimValue(string claimType)
    {
        return Principal?
            .FindFirst(claimType)?
            .Value;
    }
}
