using Codenames.Core.Domain.Enums;

namespace Codenames.Core.Domain.Guesses;

public sealed record GuessResult(int Position, GuessOutcome Outcome, Team GuessingTeam, bool TurnEnds, bool GameEnded, Team? Winner);
