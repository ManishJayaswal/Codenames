namespace Codenames.Core.Domain.Guesses;

public enum GuessOutcome
{
    FriendlyAgent, // correct team agent
    OpponentAgent, // opponent agent revealed (turn ends)
    Neutral,       // neutral revealed (turn ends)
    Assassin,      // assassin revealed (game ends, opponent wins)
    AlreadyRevealed // invalid guess attempted
}
