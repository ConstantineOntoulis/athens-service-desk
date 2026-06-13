namespace AthensServiceDesk.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string resourceName, object identifier)
        : base(
            $"{resourceName} with identifier '{identifier}' was not found.")
    {
    }
}