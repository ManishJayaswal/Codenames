namespace Codenames.Core.Domain.Exceptions;

public sealed class ClueValidationException : DomainException
{
    public ClueValidationException(string reason) : base($"Invalid clue: {reason}") { }
}
