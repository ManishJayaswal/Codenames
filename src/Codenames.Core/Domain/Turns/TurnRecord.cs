using Codenames.Core.Domain.Enums;

namespace Codenames.Core.Domain.Turns;

/// <summary>
/// Represents a complete turn in the game, including the clue and all guesses made.
/// </summary>
public sealed record TurnRecord
{
    public Team Team { get; }
    public Clue Clue { get; }
    public IReadOnlyList<GuessEvent> Guesses { get; }
    public bool TurnEndedVoluntarily { get; }
    public bool GameEnded { get; }
    public Team? Winner { get; }

    public TurnRecord(
        Team team,
        Clue clue,
        IReadOnlyList<GuessEvent> guesses,
        bool turnEndedVoluntarily = false,
        bool gameEnded = false,
        Team? winner = null)
    {
        ArgumentNullException.ThrowIfNull(clue);
        ArgumentNullException.ThrowIfNull(guesses);

        Team = team;
        Clue = clue;
        Guesses = guesses;
        TurnEndedVoluntarily = turnEndedVoluntarily;
        GameEnded = gameEnded;
        Winner = winner;
    }
}
