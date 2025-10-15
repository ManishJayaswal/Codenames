using Codenames.Core.Domain.Enums;

namespace Codenames.Core.Domain;

/// <summary>
/// Represents a single card on the Codenames board.
/// Immutable: operations that change state return a new instance.
/// </summary>
public sealed record GameCard(int Position, string Word, CardType Type, bool IsRevealed = false)
{
    public GameCard Reveal() => IsRevealed ? this : this with { IsRevealed = true };
}
