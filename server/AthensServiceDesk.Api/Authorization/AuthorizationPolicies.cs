namespace AthensServiceDesk.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string CitizenOnly =
        "CitizenOnly";

    public const string StaffOnly =
        "StaffOnly";

    public const string ManagerOrAdmin =
        "ManagerOrAdmin";
}