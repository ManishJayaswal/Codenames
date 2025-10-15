namespace Codenames.Core.Domain.Exceptions;

public sealed class InvalidGuessException : DomainException
{
    public InvalidGuessException(string message) : base(message) { }
}
