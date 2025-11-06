using Codenames.Core.Domain.Enums;
using Codenames.Core.Domain.Guesses;

namespace Codenames.Core.Domain.Turns;

/// <summary>
/// Represents a single guess made during a turn.
/// </summary>
public sealed record GuessEvent
{
    public int Position { get; }
    public GuessOutcome Outcome { get; }
    public CardType CardType { get; }
    public DateTime Timestamp { get; }

    public GuessEvent(int position, GuessOutcome outcome, CardType cardType, DateTime? timestamp = null)
    {
        if (position < 0 || position >= 25)
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 24");

        Position = position;
        Outcome = outcome;
        CardType = cardType;
        Timestamp = timestamp ?? DateTime.UtcNow;
    }
}
