namespace Codenames.Core.Domain.Exceptions;

public sealed class TeamMismatchException : DomainException
{
    public TeamMismatchException(string message) : base(message) { }
}
