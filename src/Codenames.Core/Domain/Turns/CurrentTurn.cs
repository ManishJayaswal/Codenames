using Codenames.Core.Domain.Enums;

namespace Codenames.Core.Domain.Turns;

/// <summary>
/// Tracks the current turn in progress, including the clue and guesses made so far.
/// </summary>
public sealed class CurrentTurn
{
    public Team Team { get; }
    public Clue Clue { get; }
    
    private readonly List<GuessEvent> _guesses = new();
    public IReadOnlyList<GuessEvent> Guesses => _guesses;
    
    public int GuessCount => _guesses.Count;

    public CurrentTurn(Team team, Clue clue)
    {
        ArgumentNullException.ThrowIfNull(clue);
        Team = team;
        Clue = clue;
    }

    internal void AddGuess(GuessEvent guess)
    {
        ArgumentNullException.ThrowIfNull(guess);
        _guesses.Add(guess);
    }

    /// <summary>
    /// Converts the current turn to a finalized TurnRecord.
    /// </summary>
    public TurnRecord ToRecord(bool voluntaryEnd, bool gameEnded, Team? winner)
    {
        return new TurnRecord(Team, Clue, _guesses.ToList(), voluntaryEnd, gameEnded, winner);
    }
}
