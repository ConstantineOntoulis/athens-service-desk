namespace AthensServiceDesk.Application.Common.Exceptions;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("The supplied email address or passwork is incorrect.")
    {
    }
}
