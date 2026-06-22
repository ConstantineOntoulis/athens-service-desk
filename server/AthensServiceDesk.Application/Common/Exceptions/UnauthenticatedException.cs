namespace AthensServiceDesk.Application.Common.Exceptions;

public sealed class UnauthenticatedException : Exception
{
    public UnauthenticatedException() : base(
        "The authenticated user could not be resolved.")
    {
    }
}
