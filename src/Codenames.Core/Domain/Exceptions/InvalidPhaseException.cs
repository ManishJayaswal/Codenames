namespace Codenames.Core.Domain.Exceptions;

public sealed class InvalidPhaseException : DomainException
{
    public InvalidPhaseException(string action, string expectedPhase, string actualPhase)
        : base($"Action '{action}' invalid in phase '{actualPhase}'. Expected phase: {expectedPhase}.") { }
}
